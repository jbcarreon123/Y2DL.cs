using Google.Apis.YouTube.v3;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;

namespace Y2DL.Services;

/// <summary>
/// The class for your YouTube channel info needs.
/// <seealso cref="IYoutubeService"/>
/// </summary>
public class YoutubeService : IYoutubeService
{
    private readonly List<YouTubeService> _youTubeServices;

    public YoutubeService(List<YouTubeService> youTubeServices)
    {
        _youTubeServices = youTubeServices;
    }

    public async Task<YoutubeChannel> GetChannelAsync(string channelId)
    {
        return default;
    }
}