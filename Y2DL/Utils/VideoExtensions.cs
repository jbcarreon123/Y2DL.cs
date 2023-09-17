using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Y2DL.Models;

namespace Y2DL.Utils;

public static class VideoExtensions
{
    /// <summary>
    /// Checks if it's a YouTube short.
    ///
    /// Note that YouTube shorts has:
    /// Length: &lt;=1m
    /// Size: 9:16 or any portrait video
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if the video is short or not.</returns>
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

    /// <summary>
    /// Checks if it's a ongoing livestream.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a livestream that is ongoing or not.</returns>
    public static bool IsLiveStreamOngoing(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ConcurrentViewers is not null;
    }
    
    /// <summary>
    /// Checks if the video is a livestream replay.
    /// </summary>
    /// <param name="video">the Video to check.</param>
    /// <returns>a <see cref="bool"/> object that says if it is a livestream that is a replay or not.</returns>
    public static bool IsLiveStreamReplay(this Video video)
    {
        return video.LiveStreamingDetails is not null && video.LiveStreamingDetails.ConcurrentViewers is null;
    }
}