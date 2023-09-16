using Google;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Serilog;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;
using Y2DL.Utils;

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

    public async Task<Listable<YoutubeChannel>> GetChannelsAsync(Listable<string> channelIds)
    {
        var youtubeService = _youTubeServices[0];
        List<YoutubeChannel> youtubeChannels = new List<YoutubeChannel>();

        try
        {
            youtubeChannels.AddRange(await GetChannelAsync(channelIds, youtubeService));
        }
        catch (GoogleApiException e)
        {
            if (_youTubeServices.Count is 1)
            {
                Log.Error(e, "Google API thrown an exception");
                return null;
            }

            youtubeService = _youTubeServices.Next(youtubeService);
            youtubeChannels.AddRange(await GetChannelAsync(channelIds, youtubeService));
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
        }

        return youtubeChannels;
    }

    private async Task<Listable<YoutubeChannel>?> GetChannelAsync(Listable<string> channelIds, YouTubeService youtubeService)
    {
        try
        {
            Listable<YoutubeChannel> youtubeChannels = new Listable<YoutubeChannel>();

            var listRequest = youtubeService.Channels.List("snippet,contentDetails,statistics");
            listRequest.Id = channelIds;
            var channelResponse = await listRequest.ExecuteAsync();

            List<Video> videos = new List<Video>();
            try
            {
                var playlistItems = await GetPlaylistItemsAsync(
                    new Listable<string>(channelResponse.Items.Select(x => x.ContentDetails.RelatedPlaylists.Uploads)));
                var vids = playlistItems
                    .Select(x => x.Items.Select(y => y.Snippet.ResourceId.VideoId).Take(5).ToArray())
                    .ToArray();
                var vidListRequest =
                    youtubeService.Videos.List("snippet,statistics,contentDetails,liveStreamingDetails");
                var vidIds = new Listable<string>();
                foreach (var vid in vids)
                    vidIds.AddRange(vid);
                vidListRequest.Id = vidIds;
                var videoResponse = await vidListRequest.ExecuteAsync();
                var video = videoResponse.Items;
            }
            catch
            {
            }

            foreach (var channel in channelResponse.Items)
            {
                var vid = videos.First(x => x.Snippet.ChannelId == channel.Id && !x.IsShort().Result);

                youtubeChannels.Add(new YoutubeChannel()
                {
                    Name = channel.Snippet.Title,
                    ChannelUrl = channel.Snippet.CustomUrl.IsNullOrWhitespace()
                        ? $"https://youtube.com/channel/{channel.Id}"
                        : $"{channel.Snippet.CustomUrl}",
                    Id = channel.Id,
                    Description = channel.Snippet.Description,
                    Handle = channel.Snippet.CustomUrl,
                    ChannelAvatarUrl = channel.Snippet.Thumbnails.High.Url,
                    Statistics = new Statistics()
                    {
                        Views = channel.Statistics.ViewCount.ToUlong() ?? 0,
                        Subscribers = channel.Statistics.SubscriberCount.ToUlong() ?? 0,
                        Videos = channel.Statistics.VideoCount.ToUlong() ?? 0
                    },
                    LatestVideo = new LatestVideo()
                    {
                        Description = vid.Snippet.Description ?? "or an error occured while getting latest video.",
                        Title = vid.Snippet.Title ?? "No videos",
                        Thumbnail = vid.Snippet.Thumbnails.Maxres.Url ?? "",
                        Url = $"https://youtu.be/{vid.Id}" ?? "",
                        PublishedAt = vid.Snippet.PublishedAtDateTimeOffset ?? DateTimeOffset.MinValue,
                        Statistics = new Statistics()
                        {
                            Views = vid.Statistics.ViewCount.ToUlong() ?? 0,
                            Comments = vid.Statistics.CommentCount.ToUlong() ?? 0,
                            Likes = vid.Statistics.LikeCount.ToUlong() ?? 0
                        },
                        Duration = vid.ContentDetails.Duration.ToTimeSpan().ToFormattedString()
                    }
                });
            }

            return youtubeChannels;
        }
        catch
        {
            return null;
        }
    }

    private async Task<Listable<PlaylistItems>> GetPlaylistItemsAsync(Listable<string> playlistIds)
    {
        Listable<PlaylistItems> playlistItems = new Listable<PlaylistItems>();
        
        using (var httpClient = new HttpClient())
        {
            foreach (var playlistId in playlistIds)
            {
                var playlistItemString = await httpClient.GetStringAsync(
                    $"https://yt.lemnoslife.com/playlistItems?part=snippet&playlistId={playlistId}");
                playlistItems.Add(JsonConvert.DeserializeObject<PlaylistItems>(playlistItemString));

                await Task.Delay(50);
            }
        }

        return playlistItems;
    }
}