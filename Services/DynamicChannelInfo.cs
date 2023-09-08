using Discord.WebSocket;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;

namespace Y2DL.Services;

public class DynamicChannelInfo : IY2DLService
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;

    public DynamicChannelInfo(DiscordSocketClient client, Config config)
    {
        _client = client;
        _config = config;
    }

    public async Task Run()
    {
    }
}