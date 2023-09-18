using Y2DL.Utils;
using Y2DL.Services;

namespace Y2DL.Models;

/// <summary>
/// The information of a YouTube channel.
/// <seealso cref="Y2DL.Services.YoutubeService"/>
/// </summary>
public class YoutubeChannel
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public string Handle { get; set; } = "";
    public string ChannelAvatarUrl { get; set; } = "";
    public LatestVideo? LatestVideo { get; set; } = new();
    public Statistics? Statistics { get; set; } = new();
    private string? channelUrl { get; set; } = "";
    private long created { get; set; }

    public DateTimeOffset DateCreated
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(created);
        set => created = value.ToUnixTimeMilliseconds();
    }

    public string? ChannelUrl
    {
        get => channelUrl;
        set => channelUrl = value.ToCharArray()[0] == '@'
            ? $"https://youtube.com/{value}"
            : $"https://youtube.com/channel/{value}";
    }
}

public class LatestVideo
{
    public string? Title { get; set; } = "";
    public string? Id { get; set; } = "";
    public string? Url { get; set; } = "";
    public string? Description { get; set; } = "";
    public string? Thumbnail { get; set; } = "";
    public string? Duration { get; set; } = "";
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.MinValue;
    public Statistics? Statistics { get; set; } = new();
}

public class Statistics
{
    public ulong Views { get; set; } = 0;
    public ulong Likes { get; set; } = 0;
    public ulong Comments { get; set; } = 0;
    public ulong Subscribers { get; set; } = 0;
    public ulong Videos { get; set; } = 0;

    public string? FormattedSubscribers => Subscribers.ToFormattedNumber();

    public string? FormattedViews => Views.ToFormattedNumber();

    public string? FormattedLikes => Likes.ToFormattedNumber();

    public string? FormattedComments => Comments.ToFormattedNumber();

    public string? FormattedVideos => Videos.ToFormattedNumber();
}