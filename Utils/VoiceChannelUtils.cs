using SmartFormat;
using Y2DL.Models;

namespace Y2DL.Utils;

public static class VoiceChannelUtils
{
    public static string ToFormattedName(this string str, YouTubeChannel channel)
    {
        return Smart.Format(str, channel);
    }
}