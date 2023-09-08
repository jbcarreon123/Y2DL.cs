using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Y2DL.Models;

namespace Y2DL.Services;

public class LoopService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;

    public LoopService(DiscordSocketClient client, Config config)
    {
        _client = client;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested) await Task.Delay(_config.Main.UpdateInterval, stoppingToken);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Log.Write(LogEventLevel.Warning, ex, "LoopService has thrown an exception");
        }
    }
}