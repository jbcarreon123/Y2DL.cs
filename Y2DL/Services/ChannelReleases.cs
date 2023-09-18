using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Y2DL.Database;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;
using Y2DL.Utils;

namespace Y2DL.Services;

public class ChannelReleases : IY2DLService<YoutubeChannel>
{
    private static List<(string, string)> _latestVideo { get; set; } = new();
    
    private readonly DiscordShardedClient _client;
    private readonly Config _config;
    private readonly DatabaseManager _database;

    public ChannelReleases(DiscordShardedClient client, Config config, DatabaseManager database)
    {
        _client = client;
        _config = config;
        _database = database;
    }

    public async Task RunAsync(YoutubeChannel channel)
    {
        try
        {
            if (channel.LatestVideo.Id == "")
            {
                

                foreach (var msg in _config.Services.ChannelReleases.Messages.FindAll(x => x.ChannelId == channel.Id))
                {
                    var embed = msg.Embed.ToDiscordEmbedBuilder(channel).Build();
                    
                    if (msg.Output.UseWebhook)
                    {
                        await new DiscordWebhookClient(msg.Output.WebhookUrl)
                            .SendMessageAsync(
                                msg.Content,
                                embeds: new []
                                {
                                    embed
                                }
                            );
                    }
                    else
                    {
                        await _client.GetGuild(msg.Output.GuildId).GetTextChannel(msg.Output.ChannelId)
                            .SendMessageAsync(
                                msg.Content,
                                embed: embed
                            );
                    }
                }
            }
        }
        catch
        {
            _latestVideo.Add((channel.Id, channel.LatestVideo.Url));
        }
    }
}