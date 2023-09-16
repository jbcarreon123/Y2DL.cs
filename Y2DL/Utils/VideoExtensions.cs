using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Y2DL.Models;

namespace Y2DL.Utils;

public static class VideoExtensions
{
    public static async Task<bool> IsShort(this Video video)
    {
        using (var httpClient = new HttpClient())
        {
            var videoString = await httpClient.GetStringAsync(
                $"https://yt.lemnoslife.com/videos?part=short&id={video.Id}");
            var vid = JsonConvert.DeserializeObject<Videos>(videoString);

            return vid.Items[0].Short.Available;
        }
    }

    public static bool IsLiveStreamOngoing(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ConcurrentViewers is not null;
    }
    
    public static bool IsLiveStreamReplay(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ConcurrentViewers is null;
    }
}