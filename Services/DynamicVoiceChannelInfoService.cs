using Discord;
using Discord.WebSocket;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class DynamicVoiceChannelInfoService
{
    public static async Task Run(YouTubeChannel channel, SocketGuildChannel voice, string name)
    {
        await voice.ModifyAsync(x =>
        {
            x.Name = name.ToFormattedName(channel);
        });
    }
}