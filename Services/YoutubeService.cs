using Google.Apis.Services;
using Y2DL.Models;
using Google.Apis.YouTube;
using Google.Apis.YouTube.v3;
using Y2DL.Utils;

namespace Y2DL.Services;

public class YoutubeService
{
    private YouTubeService _youTubeService;
    
    public YoutubeService(string apiKey, string apiName)
    {
        _youTubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = apiName
        });
    }

    // NOTE: THIS COSTS 3 QUOTA UNITS PER EXECUTION!
    public async Task<YouTubeChannel> GetChannel(string channelId)
    {
        var listRequest = _youTubeService.Channels.List("snippet,contentDetails,statistics");
        listRequest.Id = channelId;
        var channelResponse = await listRequest.ExecuteAsync();

        var plListRequest = _youTubeService.PlaylistItems.List("snippet,contentDetails");
        var vidListRequest = _youTubeService.Videos.List("snippet,statistics,contentDetails");

        var channel = channelResponse.Items[0];
        
        // Get latest video
        plListRequest.PlaylistId = channel.ContentDetails.RelatedPlaylists.Uploads;
        plListRequest.MaxResults = 1;
        var playlist = await plListRequest.ExecuteAsync();
        var pli = playlist.Items[0];
        vidListRequest.Id = pli.Snippet.ResourceId.VideoId;
        var video = await vidListRequest.ExecuteAsync();
        var vid = video.Items[0];

        YouTubeChannel youTubeChannel = new YouTubeChannel()
        {
            Name = channel.Snippet.Title,
            Description = channel.Snippet.Description,
            Handle = channel.Snippet.CustomUrl,
            ChannelAvatarUrl = channel.Snippet.Thumbnails.High.Url,
            Statistics = new Statistics()
            {
                Views = channel.Statistics.ViewCount.ToUlong(),
                Subscribers = channel.Statistics.SubscriberCount.ToUlong(),
                Videos = channel.Statistics.VideoCount.ToUlong()
            },
            LatestVideo = new LatestVideo()
            {
                Description = vid.Snippet.Description,
                Title = vid.Snippet.Title,
                Thumbnail = vid.Snippet.Thumbnails.Maxres.Url,
                Url = $"https://youtu.be/{pli.Snippet.ResourceId.VideoId}",
                PublishedAt = (DateTimeOffset)vid.Snippet.PublishedAtDateTimeOffset,
                Statistics = new Statistics()
                {
                    Views = vid.Statistics.ViewCount.ToUlong(),
                    Comments = vid.Statistics.CommentCount.ToUlong(),
                    Likes = vid.Statistics.LikeCount.ToUlong()
                },
                Duration = vid.ContentDetails.Duration.ToTimeSpan().ToFormattedString()
            }
        };

        return youTubeChannel;
    }
}