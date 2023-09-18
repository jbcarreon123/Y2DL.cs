using Discord.WebSocket;
using SmartFormat;
using Y2DL.Database;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;
using Y2DL.SmartFormatters;

namespace Y2DL.Services;

public class DynamicVoiceChannelInfo : IY2DLService<YoutubeChannel>
{
    private readonly DiscordShardedClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public DynamicVoiceChannelInfo(DiscordShardedClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;
    }
    
    public async Task RunAsync(YoutubeChannel youtubeChannel)
    {
        var vcs = _config.Services.DynamicChannelInfoForVoiceChannels.Channels.First(x => x.ChannelId == youtubeChannel.Id);

        foreach (var vc in vcs.VoiceChannels)
        {
            Smart.Default.AddExtensions(new LimitFormatter());
            await _client.GetGuild(vc.GuildId).GetVoiceChannel(vc.ChannelId)
                .ModifyAsync(x => x.Name = Smart.Format(vc.Name, youtubeChannel));
        }
    }
}