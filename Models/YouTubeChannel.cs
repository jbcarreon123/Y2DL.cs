using Y2DL.Utils;

namespace Y2DL.Models;

public class UpdatedChannels
{
    public string Id { get; set; }
    public YouTubeChannel Channel { get; set; }
}

public class YouTubeChannel
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public string Handle { get; set; } = "";
    public string ChannelAvatarUrl { get; set; } = "";
    public LatestVideo? LatestVideo { get; set; } = new LatestVideo();
    public Statistics? Statistics { get; set; } = new Statistics();
    private string? channelUrl { get; set; } = "";
    private long created { get; set; } = 0;
    public DateTimeOffset DateCreated
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(created);
        set => created = value.ToUnixTimeMilliseconds();
    }
    public string? ChannelUrl
    {
        get => channelUrl;
        set => channelUrl = value.ToCharArray()[0] == '@' ? $"https://youtube.com/{value}" : $"https://youtube.com/channel/{value}";
    }
}

public class LatestVideo
{
    public string? Title { get; set; } = "";
    public string? Url { get; set; } = "";
    public string? Description { get; set; } = "";
    public string? Thumbnail { get; set; } = "";
    public string? Duration { get; set; } = "";
    public DateTimeOffset PublishedAt { get; set; } = DateTimeOffset.MinValue;
    public Statistics? Statistics { get; set; } = new Statistics();
}

public class Statistics
{
    public ulong Views { get; set; } = 0;
    public ulong Likes { get; set; } = 0;
    public ulong Comments { get; set; } = 0;
    public ulong Subscribers { get; set; } = 0;
    public ulong Videos { get; set; } = 0;

    public string? FormattedSubscribers
    {
        get => Subscribers.ToFormattedNumber();
    }
    public string? FormattedViews
    {
        get => Views.ToFormattedNumber();
    }
    public string? FormattedLikes
    {
        get => Likes.ToFormattedNumber();
    }
    public string? FormattedComments
    {
        get => Comments.ToFormattedNumber();
    }
    public string? FormattedVideos
    {
        get => Videos.ToFormattedNumber();
    }
}