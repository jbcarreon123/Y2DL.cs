using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SmartFormat;
using Y2DL.Database;
using Y2DL.Logging;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;
using Y2DL.Services;
using Y2DL.Services.DiscordCommandsService;
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
    
    private static readonly int _numOfShards = 5;

    private static IServiceProvider CreateProvider()
    {
        var discordSocketConfig = new DiscordSocketConfig
        {
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.Guilds,
            LogLevel = LogSeverity.Debug,
            TotalShards = _numOfShards
        };

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
        
        Log.Logger = _serviceProvider.GetRequiredService<Logger>();

        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        var version = fileVersionInfo.ProductVersion;
        await LogAsync(new LogMessage(LogSeverity.Info, "Y2DL", $"Y2DL v{version} (formerly as YTSCTD)"));
        await LogAsync(new LogMessage(LogSeverity.Info, "Y2DL", "This program comes with ABSOLUTELY NO WARRANTY!"));
        await LogAsync(new LogMessage(LogSeverity.Info, "Y2DL",
            "  > https://github.com/jbcarreon123/YTSCTD/blob/main/LICENSE"));

        if (version != config.Version)
        {
            await LogAsync(new LogMessage(LogSeverity.Warning, "Y2DL",
                "The config file version is different than the program version"));
            await LogAsync(new LogMessage(LogSeverity.Warning, "Y2DL",
                "Update your config on https://jbcarreon123.github.io/y2dl/config"));
        }

        client.Log += LogAsync;
        client.ShardReady += ReadyAsync;
        
        await commands.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, config.Main.BotConfig.BotToken);
        await client.StartAsync();

        await Task.Delay(5000);
        await commands.RegisterAsync();

        await Task.Delay(Timeout.Infinite);
    }
    
    private static int _readyShards = 0;
    private async Task ReadyAsync(DiscordSocketClient client)
    { 
        _readyShards++;
        
        if (_readyShards >= _numOfShards)
        {
            var loopService = _serviceProvider.GetRequiredService<LoopService>();
            await loopService.StartAsync(CancellationToken.None);
            Log.Information("All shards are now ready.");
            return;
        }
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