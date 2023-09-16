using Discord;
using Discord.WebSocket;
using Y2DL.Attributes;
using Y2DL.Database;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;

namespace Y2DL.Services;

[Y2DLService("Y2DL.DynamicChannelInfo")]
public class DynamicChannelInfo : IY2DLService
{
    private readonly DiscordSocketClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public DynamicChannelInfo(DiscordSocketClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;
    }

    public async Task RunAsync(YoutubeChannel youtubeChannel)
    {
        
    }
}