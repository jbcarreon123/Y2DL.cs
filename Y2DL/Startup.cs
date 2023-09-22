using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SmartFormat;
using Y2DL.Database;
using Y2DL.Logging;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;
using Y2DL.Plugins;
using Y2DL.Plugins.Interfaces;
using Y2DL.Services;
using Y2DL.Services.DiscordCommands;
using Y2DL.SmartFormatters;
using Y2DL.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ChannelReleases = Y2DL.Services.ChannelReleases;
using YoutubeService = Y2DL.Services.YoutubeService;

namespace Y2DL;

public class Startup
{
    private readonly IServiceProvider _serviceProvider = CreateProvider();
    
    /// <summary>
    /// The entry point of Y2DL.
    /// Note that this isn't a single-threaded program,
    /// so don't put anything single-threaded below this (like [STAThread])
    /// </summary>
    private static void Main(string[] args)
    {
        new Startup().RunAsync(args).GetAwaiter().GetResult();
    }
    
    private static readonly int _numOfShards = 1;

    private static IServiceProvider CreateProvider()
    {
        var configFile = File.ReadAllText("Config/Y2DLConfig.yml");
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStringEnumConverter())
            .Build();
        var appConfig = deserializer.Deserialize<Config>(configFile);
        
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Discord(appConfig)
            .CreateLogger();
        
        var asm = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(asm.Location);
        var version = fileVersionInfo.ProductVersion;
        logger.Information("Y2DL v{0} (formerly as YTSCTD)", version);
        logger.Information("This program comes with ABSOLUTELY NO WARRANTY!");
        logger.Information("  > https://github.com/jbcarreon123/YTSCTD/blob/main/LICENSE");
        
        var discordSocketConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.Guilds,
            LogLevel = LogSeverity.Debug,
            TotalShards = _numOfShards
        };

        List<YouTubeService> youTubeServices = new();
        foreach (var apiKey in appConfig.Main.ApiKeys)
            youTubeServices.Add(new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = apiKey.YoutubeApiKey,
                ApplicationName = apiKey.YoutubeApiName
            }));
        
        var collection = new ServiceCollection()
            .AddDbContext<Y2dlDbContext>()
            .AddScoped<YoutubeService>()
            .AddSingleton<DynamicChannelInfo>()
            .AddSingleton<DynamicVoiceChannelInfo>()
            .AddSingleton<ChannelReleases>()
            .AddSingleton(discordSocketConfig)
            .AddSingleton(appConfig)
            .AddSingleton(logger)
            .AddSingleton(youTubeServices)
            .AddSingleton<DiscordShardedClient>()
            .AddSingleton<InteractionService>()
            .AddSingleton<InteractionHandler>()
            .AddSingleton<LoopService>()
            .AddSingleton<DatabaseManager>();

        return collection.BuildServiceProvider();
    }

    private async Task RunAsync(string[] args)
    {
        var client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
        var config = _serviceProvider.GetRequiredService<Config>();
        var commands = _serviceProvider.GetRequiredService<InteractionHandler>();
        var database = _serviceProvider.GetRequiredService<DatabaseManager>();
        Log.Logger = _serviceProvider.GetRequiredService<Logger>();

        await PluginManager.LoadPluginsAsync(commands, _serviceProvider);

        database.Configure();

        client.Log += LogAsync;
        client.MessageReceived += MessageReceived;
        client.ShardReady += ReadyAsync;
        
        await commands.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, config.Main.BotConfig.BotToken);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }
    
    private static int _readyShards = 0;
    private async Task ReadyAsync(DiscordSocketClient client)
    { 
        _readyShards++;
        
        if (_readyShards >= _numOfShards)
        {
            var loopService = _serviceProvider.GetRequiredService<LoopService>();
            var commands = _serviceProvider.GetRequiredService<InteractionHandler>();
            await loopService.StartAsync(CancellationToken.None);
            await commands.RegisterAsync();
            Log.Information("All shards are now ready.");
        }
    }

    private async Task MessageReceived(SocketMessage message)
    {
        var youtubeService = _serviceProvider.GetRequiredService<YoutubeService>();
        var client = _serviceProvider.GetRequiredService<DiscordShardedClient>();

        try
        {
            if (message is IUserMessage userMessage)
            {
                if (message.MentionedUsers.Any(x => x.Id == client.CurrentUser.Id))
                {
                    var idType = message.Content.GetYouTubeIdAndType();

                    switch (idType.Type)
                    {
                        case "Video":
                            await userMessage.ReplyAsync(embed: await youtubeService.GetVideoAsync(idType.Id));
                            break;
                        case "Playlist":
                            await userMessage.ReplyAsync(embed: await youtubeService.GetPlaylistAsync(idType.Id));
                            break;
                        case "Channel":
                            await userMessage.ReplyAsync(embed: await youtubeService.GetChannelAsync(idType.Id));
                            break;
                        default:
                            await userMessage.ReplyAsync(embed: EmbedUtils.GenerateHelpEmbed());
                            break;
                    }
                }
            }
        }
        catch { }
    }
    
    private async Task LogAsync(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        Log.Write(severity, message.Exception, $"{message.Source}: {message.Message}");
        await Task.CompletedTask;
    }
}