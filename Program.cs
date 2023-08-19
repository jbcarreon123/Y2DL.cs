using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Webhook;
using Discord.WebSocket;
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
    public static List<(Channels, IChannel)> VoiceChannels { get; set; }
    public static DatabaseManager Database = new DatabaseManager("Database/Database.db3");
    private static DiscordSocketClient _client { get; set; }
    private static CancellationTokenSource _cancellationTokenSource { get; set; }
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
        _youtubeService = new YoutubeService(Config.Main.YoutubeApiKey, Config.Main.YoutubeApiName);

        await Task.Delay(-1);
    }

    private static async Task InitializeBot()
    {
        DiscordSocketConfig config = new() {
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            LogLevel = (LogSeverity)Config.Main.LogLevel
        };
        _client = new DiscordSocketClient(config);
        _client.Log += Log;
        _client.Ready += Ready;
        _client.LoggedOut += () =>
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        };
        _client.LoggedIn += async () =>
        {
            if (_botReady)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                await DynamicChannelInfoService.Run(cancellationToken, _youtubeService);
            }
        };
        _client.MessageDeleted += async (cacheable, cacheable1) =>
        {
            Database.Remove(cacheable.Id);
        };
        await _client.LoginAsync(TokenType.Bot, Config.Main.BotConfig.BotToken);
        await _client.StartAsync();
        
        _commands = new CommandInterface(_client, new InteractionService(_client.Rest));
        await _commands.InitializeAsync();
    }

    private static async Task Ready()
    {
        _botReady = true;

        WebhookClients = InitializeMessageBasedServices();
        
        if (Config.Services.Commands.Enabled)
            await _commands.GetInteractionService().RegisterCommandsGloballyAsync(true);

        if (Config.Services.DynamicChannelInfoForVoiceChannels.Enabled)
            VoiceChannels = InitializeChannels();

        await _client.SetStatusAsync((UserStatus)Config.Main.BotConfig.State);
        if (Config.Main.BotConfig.Status.Enabled)
        {
            var status = Config.Main.BotConfig.Status.Emoji + " " ?? "";
            status += Config.Main.BotConfig.Status.Text ?? "";
            await _client.SetCustomStatusAsync(status);
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        async void Action()
        {
            await DynamicChannelInfoService.Run(cancellationToken, _youtubeService);
        }

        var DataInThread = new Thread(Action)
        {
            IsBackground = true
        };
        DataInThread.Start();
    }

    public static SocketTextChannel GetChannel(ulong guildId, ulong channelId)
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
                        ? (message, new Output(new DiscordWebhookClient(message.Output.WebhookUrl)))
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
                dyn.Add(
                    message.Output.UseWebhook
                        ? (message, new Output(new DiscordWebhookClient(message.Output.WebhookUrl)))
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

    public static List<(Channels, IChannel)> InitializeChannels()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        List<(Channels, IChannel)> chn = new List<(Channels, IChannel)>();
        if (Config.Services.ChannelReleases.Enabled)
        {
            foreach (Channels channel in Config.Services.DynamicChannelInfoForVoiceChannels.Channels)
            {
                foreach (VoiceChannels voiceChannel in channel.VoiceChannels)
                {
                    chn.Add(
                        (channel, _client.GetGuild(voiceChannel.GuildId).GetChannel(voiceChannel.ChannelId))
                    );
                }
            }
        }

        Log(new LogMessage(LogSeverity.Debug, "Y2DL",
            $"Initialized VoiceChannels: {stopwatch.ElapsedMilliseconds}ms"));
        stopwatch.Stop();
        return chn;
    }

    public static Task Log(LogMessage message)
    {
        Console.WriteLine(
            $"{DateTime.Now:MM/dd/yy hh:mm:ss tt} | " +
            $"{message.Severity.ToString()} | " +
            $"{message.Source} | " +
            $"{(String.IsNullOrWhiteSpace(message.Message)? message.Exception.Message : message.Message)}" +
            $"{(message.Exception is not null? $" | {message.Exception}" : "")}"
        );
        
        return Task.CompletedTask;
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
        public Output(DiscordWebhookClient webhook)
        {
            Webhook = webhook;
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