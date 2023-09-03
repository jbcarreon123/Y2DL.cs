using Discord;
using Discord.Webhook;
using Google;
using Google.Apis.Services;
using Y2DL.Models;
using Google.Apis.YouTube;
using Google.Apis.YouTube.v3;
using Y2DL.Utils;

namespace Y2DL.Services;

public class YoutubeService
{
    private YouTubeService _youTubeService;
    private List<UpdatedChannels> _updatedChannels = new List<UpdatedChannels>();
    
    public YoutubeService(string apiKey, string apiName)
    {
        _youTubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = apiName
        });
    }

    public void ResetChannels()
    {
        _updatedChannels = new List<UpdatedChannels>();
    }

    // NOTE: THIS COSTS 3 QUOTA UNITS PER CHANNEL!
    public async Task<YouTubeChannel> GetChannel(string channelId)
    {
        try
        {
            if (!_updatedChannels.Exists(x => x.Id == channelId))
            {
                var listRequest = _youTubeService.Channels.List("snippet,contentDetails,statistics");
                listRequest.Id = channelId;
                var channelResponse = await listRequest.ExecuteAsync();

                var plListRequest = _youTubeService.PlaylistItems.List("snippet,contentDetails");
                var vidListRequest = _youTubeService.Videos.List("snippet,statistics,contentDetails");

                var channel = channelResponse.Items[0];

                // Get latest video
                plListRequest.PlaylistId = channel.ContentDetails.RelatedPlaylists.Uploads;
                plListRequest.MaxResults = 2;
                var playlist = await plListRequest.ExecuteAsync();
                var pli = playlist.Items[0];
                vidListRequest.Id = pli.Snippet.ResourceId.VideoId;
                var video = await vidListRequest.ExecuteAsync();
                var vid = video.Items[0];

                YouTubeChannel youTubeChannel = new YouTubeChannel()
                {
                    Name = channel.Snippet.Title,
                    Id = channel.Id,
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

            return _updatedChannels.First(x => x.Id == channelId).Channel;
        }
        catch (GoogleApiException e)
        {
            await Program.Log(new LogMessage(LogSeverity.Warning, "YouTube",
                "Quota exceeded.", e));

            if (Program.Config.Main.ApiKeys.Count is 1)
            {
                await Program.Log(new LogMessage(LogSeverity.Critical, "YouTube",
                    "User has not put a different API key."));
                await Program.Log(new LogMessage(LogSeverity.Warning, "Y2DL",
                    "Y2DL is disabled by: YouTubeService."));
                await Program.Log(new LogMessage(LogSeverity.Warning, "Y2DL",
                    "  > To enable it, please run `/enable y2dl`."));
                await Program.GetCurrentUserClient().SetCustomStatusAsync("Y2DL disabled.");
                LoopService.CancelTask = true;
                return null;
            }

            await Program.Log(new LogMessage(LogSeverity.Info, "YouTube", "Switching to a different API Key"));

            ApiKeys apiKey = Program.Config.Main.ApiKeys.LoopAbout(
                Program.Config.Main.ApiKeys.IndexOf(
                    Program.Config.Main.ApiKeys.First(x => x.YoutubeApiKey == _youTubeService.ApiKey)));
            new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey.YoutubeApiKey,
                ApplicationName = apiKey.YoutubeApiName
            });

            return await GetChannel(channelId);
        }
        catch (Exception e)
        {
            await Program.Log(new LogMessage(LogSeverity.Warning, "YouTube",
                "An error occured.", e));
        }

        return null;
    }
}