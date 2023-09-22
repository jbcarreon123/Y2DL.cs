using Discord.WebSocket;
using Y2DL.Database;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;

namespace Y2DL.Services;

public class LinkedSubscriberRoles
{
    private readonly DiscordShardedClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public LinkedSubscriberRoles(DiscordShardedClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;

        _client.ButtonExecuted += LSR_ButtonExecuted;
    }

    public async Task LSR_ButtonExecuted(SocketMessageComponent component)
    {
        
    }
}