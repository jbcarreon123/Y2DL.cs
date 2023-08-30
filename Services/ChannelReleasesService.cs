using Discord;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class ChannelReleasesService
{
    private static List<(string, string)> _latestVideo { get; set; } = new();

    public static async Task Run(YouTubeChannel channel, (Message, Program.Output) message)
    {
        try
        {
            if (channel.LatestVideo.Url != _latestVideo.First(x => x.Item1 == channel.Id).Item2)
            {
                _latestVideo.Remove(_latestVideo.First(x => x.Item1 == channel.Id));
                _latestVideo.Add((channel.Id, channel.LatestVideo.Url));

                if (!message.Item1.Output.UseWebhook)
                {
                    await Program.GetTextChannel(message.Item2.GuildId, message.Item2.ChannelId).SendMessageAsync(
                        message.Item1.Content,
                        embed: message.Item1.Embed.ToDiscordEmbedBuilder(channel).Build()
                    );
                }
                else
                {
                    await message.Item2.Webhook.SendMessageAsync(
                        message.Item1.Content,
                        embeds: new[]
                        {
                            message.Item1.Embed.ToDiscordEmbedBuilder(channel).Build()
                        }
                    );
                }
            }
        }
        catch
        {
            _latestVideo.Add((channel.Id, channel.LatestVideo.Url));
        }
    }
}