using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;

namespace Y2DL.Services;

public class LoopService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;
    private readonly DynamicChannelInfo _dynamicChannelInfo;

    public LoopService(DiscordSocketClient client, Config config, DynamicChannelInfo dynamicChannelInfo)
    {
        _client = client;
        _config = config;
        _dynamicChannelInfo = dynamicChannelInfo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                              
                
                await Task.Delay(_config.Main.UpdateInterval, stoppingToken);
            }
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