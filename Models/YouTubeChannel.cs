namespace Y2DL.Models;

public class YouTubeChannel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Handle { get; set; }
    public string ChannelAvatarUrl { get; set; }
    public LatestVideo LatestVideo { get; set; }
    public Statistics Statistics { get; set; }
    private string channelUrl { get; set; }
    private long created { get; set; }
    public DateTimeOffset DateCreated
    {
        get => DateTimeOffset.FromUnixTimeMilliseconds(created);
        set => created = value.ToUnixTimeMilliseconds();
    }
    public string ChannelUrl
    {
        get => channelUrl;
        set => channelUrl = value.ToCharArray()[0] == '@' ? $"https://youtube.com/{value}" : $"https://youtube.com/channel/{value}";
    }
}

public class LatestVideo
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Description { get; set; }
    public string Thumbnail { get; set; }
    public string Duration { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public Statistics Statistics { get; set; }
}

public class Statistics
{
    public ulong Views { get; set; } = 0;
    public ulong Likes { get; set; } = 0;
    public ulong Comments { get; set; } = 0;
    public ulong Subscribers { get; set; } = 0;
    public ulong Videos { get; set; } = 0;
}