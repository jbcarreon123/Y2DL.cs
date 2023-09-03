using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Webhook;
using Discord.WebSocket;
using Google.Apis.YouTube.v3;
using Y2DL.Models;
using YamlDotNet.Serialization.Converters;
using Y2DL.Services;
using Y2DL.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Y2DL;

public class Program
{
    public static Config Config = LoadConfigFile("Config/Y2DLConfig.yml");
    public static (List<(Message, Output)>, List<(Message, Output)>) WebhookClients { get; set; }
    public static List<(VoiceChannels, SocketGuildChannel, string)> VoiceChannels { get; set; }
    public static DatabaseManager Database = new DatabaseManager("Database.db3");
    public static int LogoutThreshold = 0;
    private static DiscordSocketClient _client { get; set; }
    private static CommandInterface _commands { get; set; }
    private static bool _botReady = false;
    private static YoutubeService _youtubeService { get; set; }

    public static async Task Main(string[] args)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        string version = fileVersionInfo.ProductVersion;
        await Log(new LogMessage(LogSeverity.Info, "Y2DL", $"Y2DL v{version} (formerly as YTSCTD)"));
        await Log(new LogMessage(LogSeverity.Info, "Y2DL", $"This program comes with ABSOLUTELY NO WARRANTY!"));
        await Log(new LogMessage(LogSeverity.Info, "Y2DL", $"  > https://github.com/jbcarreon123/YTSCTD/blob/main/LICENSE"));
        
        await Log(new LogMessage(LogSeverity.Info, "Y2DL", $"Mode: {Config.Main.Type.ToString()}"));

        if (version != Config.Version)
        {
            await Log(new LogMessage(LogSeverity.Warning, "Y2DL",
                "The config file version is different than the program version"));
            await Log(new LogMessage(LogSeverity.Warning, "Y2DL",
                "Update your config on https://jbcarreon123.github.io/y2dl/config"));
        }

        switch (Config.Main.Type)
        {
            case AppType.Webhook:
                WebhookClients = InitializeMessageBasedServices();
                break;
            case AppType.Bot:
                await InitializeBot();
                break;
        }

        await Database.Initialize();
        _youtubeService = new YoutubeService(Config.Main.ApiKeys[0].YoutubeApiKey, Config.Main.ApiKeys[0].YoutubeApiName);

        await Task.Delay(-1);
    }

    public static SocketSelfUser GetCurrentUser()
    {
        return _client.CurrentUser;
    }
    
    public static DiscordSocketClient GetCurrentUserClient()
    {
        return _client;
    }

    private static async Task InitializeBot()
    {
        DiscordSocketConfig config = new() {
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.Guilds,
            LogLevel = (LogSeverity)Config.Main.Logging.LogLevel
        };
        _client = new DiscordSocketClient(config);
        _client.Log += Log;
        _client.Ready += Ready;
        _client.Disconnected += async (e) =>
        {
            await Log(new LogMessage(LogSeverity.Error, "Discord", "Client disconnected", e));
            LoopService.CancelTask = true;
        };
        _client.Connected += async () =>
        {
            if (_botReady)
            {
                async void Action()
				{
					LoopService.CancelTask = false;
					await LoopService.Run(_youtubeService);
				}

				var DataInThread = new Thread(Action)
				{
					IsBackground = true
				};
				DataInThread.Start();
            }
        };
        _client.MessageDeleted += async (cacheable, cacheable1) => { Database.Remove(cacheable.Id); };
        await _client.LoginAsync(TokenType.Bot, Config.Main.BotConfig.BotToken);
        await _client.StartAsync();
    }

    private static async Task Ready()
    {
        _botReady = true;
        
        _commands = new CommandInterface(_client, new InteractionService(_client.Rest));
        await _commands.InitializeAsync();

        WebhookClients = InitializeMessageBasedServices();

        if (Config.Services.Commands.Enabled)
            await _commands.GetInteractionService().RegisterCommandsGloballyAsync(true);

        if (Config.Services.DynamicChannelInfoForVoiceChannels.Enabled)
            VoiceChannels = InitializeChannels();

        await _client.SetStatusAsync((UserStatus)Config.Main.BotConfig.State);

        async void Action()
        {
            LoopService.CancelTask = false;
            await LoopService.Run(_youtubeService);
        }

        var DataInThread = new Thread(Action)
        {
            IsBackground = true
        };
        DataInThread.Start();
    }
    
    public static YoutubeService GetYoutubeService()
    {
        return _youtubeService;
    }

    public static SocketTextChannel GetTextChannel(ulong guildId, ulong channelId)
    {
        return _client.GetGuild(guildId).GetTextChannel(channelId);
    }
    
    public static SocketTextChannel GetVoiceChannel(ulong guildId, ulong channelId)
    {
        return _client.GetGuild(guildId).GetTextChannel(channelId);
    }

    public static (List<(Message, Output)>, List<(Message, Output)>) InitializeMessageBasedServices()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        List<(Message, Output)> dyn = new List<(Message, Output)>();
        if (Config.Services.DynamicChannelInfo.Enabled)
        {
            foreach (Message message in Config.Services.DynamicChannelInfo.Messages)
            {
                dyn.Add(
                    message.Output.UseWebhook
                        ? (message, new Output(new DiscordWebhookClient(message.Output.WebhookUrl), message.Output.ChannelId))
                        : (message,
                            new Output(message.Output.GuildId, message.Output.ChannelId))
                );
            }
        }
        
        List<(Message, Output)> chn = new List<(Message, Output)>();
        if (Config.Services.ChannelReleases.Enabled)
        {
            foreach (Message message in Config.Services.ChannelReleases.Messages)
            {
                chn.Add(
                    message.Output.UseWebhook
                        ? (message, new Output(new DiscordWebhookClient(message.Output.WebhookUrl), message.Output.ChannelId))
                        : (message,
                            new Output(message.Output.GuildId, message.Output.ChannelId))
                );
            }
        }

        Log(new LogMessage(LogSeverity.Debug, "Y2DL",
            $"Initialized Webhooks: {stopwatch.ElapsedMilliseconds}ms"));
        stopwatch.Stop();
        return (dyn, chn);
    }

    public static List<(VoiceChannels, SocketGuildChannel, string)> InitializeChannels()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        List<(VoiceChannels, SocketGuildChannel, string)> chn = new List<(VoiceChannels, SocketGuildChannel, string)>();
        if (Config.Services.DynamicChannelInfoForVoiceChannels.Enabled)
        {
            foreach (Channels channel in Config.Services.DynamicChannelInfoForVoiceChannels.Channels)
            {
                foreach (VoiceChannels voiceChannel in channel.VoiceChannels)
                {
                    chn.Add(
                        (voiceChannel, _client.GetGuild(voiceChannel.GuildId).GetChannel(voiceChannel.ChannelId), channel.ChannelId)
                    );
                }
            }
        }

        Log(new LogMessage(LogSeverity.Debug, "Y2DL",
            $"Initialized VoiceChannels: {stopwatch.ElapsedMilliseconds}ms"));
        stopwatch.Stop();
        return chn;
    }

    public static async Task Log(LogMessage message)
    {
        Console.WriteLine(
            $"{DateTime.Now:MM/dd/yy hh:mm:ss tt} | " +
            $"{message.Severity.ToString()} | " +
            $"{message.Source} | " +
            $"{(String.IsNullOrWhiteSpace(message.Message)? message.Exception.Message : message.Message)}" +
            $"{(message.Exception is not null? $" | {message.Exception}" : "")}"
        );

        if (message.Severity is LogSeverity.Error or LogSeverity.Critical or LogSeverity.Warning)
        {
            var embed = new EmbedBuilder()
            {
                Title = $"{message.Severity.ToString()} | {message.Source}",
                Description = $"{message.Message}"
            };

            if (Config.Main.Logging.LogErrorChannel.UseWebhook)
            {
                var webhook = new DiscordWebhookClient(Config.Main.Logging.LogErrorChannel.WebhookUrl);
                await webhook.SendMessageAsync(embeds: new[]
                {
                    embed.Build()                
                });
            }
            else
            {
                await GetTextChannel(Config.Main.Logging.LogErrorChannel.GuildId, Config.Main.Logging.LogErrorChannel.ChannelId).SendMessageAsync(embeds: new[]
                {
                    embed.Build()                
                });
            }
        }
    }

    public static Config LoadConfigFile(string path)
    {
        string yamlContent = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();

        return deserializer.Deserialize<Config>(yamlContent);
    }

    public class Output
    {
        public Output(DiscordWebhookClient webhook, ulong channelId)
        {
            Webhook = webhook;
            ChannelId = channelId;
        }

        public Output(ulong guildId, ulong channelId)
        {
            GuildId = guildId;
            ChannelId = channelId;
        }

        public DiscordWebhookClient? Webhook { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}