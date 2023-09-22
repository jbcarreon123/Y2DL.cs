using System.ServiceModel.Syndication;
using System.Xml;
using Discord;
using Google;
using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Serilog;
using Y2DL.Models;
using Y2DL.ServiceInterfaces;
using Y2DL.Utils;
using Embed = Y2DL.Models.Embed;

namespace Y2DL.Services;

/// <summary>
/// The class for your YouTube channel info needs.
/// <seealso cref="IYoutubeService"/>
/// </summary>
public class YoutubeService : IYoutubeService
{
    private readonly List<YouTubeService> _youTubeServices;
    private YouTubeService _youTubeService;
    private readonly Config _config;

    public YoutubeService(List<YouTubeService> youTubeServices, Config config)
    {
        _youTubeServices = youTubeServices;
        _config = config;
        _youTubeService = youTubeServices[0];
    }

    public async Task<Listable<YoutubeChannel>> GetChannelsAsync(Listable<string> channelIds)
    {
        List<YoutubeChannel> youtubeChannels = new List<YoutubeChannel>();

        try
        {
            youtubeChannels.AddRange(await GetChannelAsync(channelIds));
        }
        catch (GoogleApiException e)
        {
            if (_youTubeServices.Count is 1)
            {
                Log.Error(e, "Google API thrown an exception, possibly API quota exceeded");
                return null;
            }

            _youTubeService = _youTubeServices.Next(_youTubeService);
            _youTubeServices.MoveFirstToLast();
            youtubeChannels.AddRange(await GetChannelAsync(channelIds));
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
        }

        return youtubeChannels;
    }

    private async Task<Listable<YoutubeChannel>?> GetChannelAsync(Listable<string> channelIds)
    {
        try
        {
            Listable<YoutubeChannel> youtubeChannels = new Listable<YoutubeChannel>();

            var listRequest = _youTubeService.Channels.List("snippet,contentDetails,statistics");
            listRequest.Id = channelIds;
            var channelResponse = await listRequest.ExecuteAsync();

            List<Video> videos = new List<Video>();
            try
            {
                var video = await GetLatestVideoForChannelsAsync(channelIds);
                videos.AddRange(video);
            }
            catch (Exception e)
            {
                Log.Warning(e, "An error occured while getting videos");
            }

            foreach (var channel in channelResponse.Items)
            {
                Video vid = new Video();
                try
                {
                    vid = videos.First(x => x.Snippet.ChannelId == channel.Id);
                }
                catch
                {
                    vid = new Video()
                    {
                        Id = null,
                        Snippet = new VideoSnippet()
                        {
                            Description = null,
                            Title = null,
                            PublishedAtDateTimeOffset = DateTimeOffset.MinValue,
                            Thumbnails = new ThumbnailDetails()
                            {
                                Maxres = new Thumbnail()
                                {
                                    Url = null
                                }
                            }
                        },
                        ContentDetails = new VideoContentDetails()
                        {
                            Duration = null
                        },
                        Statistics = new VideoStatistics()
                        {
                            CommentCount = null,
                            LikeCount = null,
                            ViewCount = null
                        }
                    };
                }

                var ytChannel = new YoutubeChannel()
                {
                    Name = channel.Snippet.Title,
                    ChannelUrl = channel.Url(),
                    Id = channel.Id,
                    Description = channel.Snippet.Description,
                    Handle = channel.Snippet.CustomUrl,
                    ChannelAvatarUrl = channel.Snippet.Thumbnails.High.Url,
                    Statistics = new Statistics()
                    {
                        Views = channel.Statistics.ViewCount.ToUlong() ?? 0,
                        Subscribers = channel.Statistics.SubscriberCount.ToUlong() ?? 0,
                        Videos = channel.Statistics.VideoCount.ToUlong() ?? 0
                    }
                };

                try
                {
                    ytChannel.LatestVideo = new LatestVideo()
                    {
                        Id = vid.Id ?? "",
                        ChannelId = vid.Snippet.ChannelId,
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
                    };
                } catch {}

                if (vid.IsLiveStreamOngoing())
                    ytChannel.LatestVideo.Statistics.ConcurrentLiveViewers = vid.LiveStreamingDetails.ConcurrentViewers.ToUlong() ?? 0;

                youtubeChannels.Add(ytChannel);
            }

            return youtubeChannels;
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
            
            return null;
        }
    }

    public async Task<Listable<PlaylistItem>> GetPlaylistItemsAsync(Listable<string> playlistIds)
    {
        Listable<PlaylistItem> playlistItems = new Listable<PlaylistItem>();

        foreach (var playlistId in playlistIds)
        {
            if (playlistId.IsNullOrWhitespace())
                continue;

            try
            {
                var playlistItemsRequest = _youTubeService.PlaylistItems.List("snippet,contentDetails");
                playlistItemsRequest.MaxResults = 50;
                
                // if it's a youtube channel id
                if (playlistId.StartsWith("UC"))
                {
                    // Gets normal videos (1 quota unit)
                    List<PlaylistItem> plys = new List<PlaylistItem>();
                    playlistItemsRequest.PlaylistId = "UULF" + playlistId.Substring(2);
                    var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                    plys.AddRange(playlistItemsResponse.Items.Take(1));

                    // Gets streams (+1 quota unit)
                    if (_config.Main.ChannelConfig.EnableStreams)
                    {
                        try
                        {
                            playlistItemsRequest.PlaylistId = "UULV" + playlistId.Substring(2);
                            playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                            plys.AddRange(playlistItemsResponse.Items.Take(1));
                        }
                        catch
                        {
                        }
                    }

                    // Gets shorts (+1 quota unit)
                    if (_config.Main.ChannelConfig.EnableShorts)
                    {
                        try
                        {
                            playlistItemsRequest.PlaylistId = "UUSH" + playlistId.Substring(2);
                            playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                            plys.AddRange(playlistItemsResponse.Items.Take(1));
                        }
                        catch
                        {
                        }
                    }

                    playlistItems.AddRange(plys
                        .OrderByDescending(item => item.ContentDetails.VideoPublishedAtDateTimeOffset).Take(1));
                }
                else
                {
                    playlistItemsRequest.PlaylistId = playlistId;
                    var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                    playlistItems.AddRange(playlistItemsResponse.Items);
                }

            }
            catch (Exception e)
            {
                Log.Warning("An exception occured while getting playlist items.", e);
            }
        }

        return playlistItems;
    }

    private async Task<List<Video>> GetLatestVideoForChannelsAsync(Listable<string> channelIds)
    {
        List<string> videoIds = new List<string>();
        
        // Method 1 (RSS)
        // Much less quota used, but includes shorts
        /*
        foreach (var channelId in channelIds)
        {
            try
            {
                var url = "https://www.youtube.com/feeds/videos.xml?channel_id=" + channelId;
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);

                videoIds.Add(feed.Items.ToList()[0].Links.ToList()[0].Uri.AbsoluteUri.GetYouTubeId());
            }
            catch
            {

            }
        }
        */
        
        // Method 2 (API)
        // Much more quota used, but excludes shorts
        var plItems = await GetPlaylistItemsAsync(channelIds);
        videoIds.AddRange(plItems.Select(x => x.ContentDetails.VideoId));

        return await GetVideosAsync(videoIds);
    }

    public async Task<Playlist> GetPlaylistInfoAsync(string playlistId)
    {
        var plListRequest = _youTubeService.Playlists.List("snippet,contentDetails,status");
        plListRequest.Id = playlistId;

        var playlistListResponse = await plListRequest.ExecuteAsync();
        return playlistListResponse.Items[0];
    }

    public async Task<List<Video>> GetVideosAsync(Listable<string> videoIds)
    {
        var vidListRequest =
            _youTubeService.Videos.List("snippet,statistics,contentDetails,liveStreamingDetails");
        vidListRequest.Id = videoIds;
        var videoResponse = await vidListRequest.ExecuteAsync();
        return videoResponse.Items.ToList();
    }
    
        public async Task<Discord.Embed> GetChannelAsync(string channelId)
    {
        var chnl = await GetChannelsAsync(channelId);
        var channel = chnl[0];

        var embed = new EmbedBuilder()
            {
                Title = $"{channel.Name} ({channel.Handle})",
                Url = channel.ChannelUrl,
                Description = $"{channel.Description.Limit(100)}",
                ThumbnailUrl = channel.ChannelAvatarUrl
            }
            .AddField("Subscribers", channel.Statistics.FormattedSubscribers, true)
            .AddField("Views", channel.Statistics.FormattedViews, true)
            .AddField("Videos", channel.Statistics.FormattedVideos, true);

        try
        {
            double ctl = (double)channel.LatestVideo.Statistics.Likes / (double)channel.LatestVideo.Statistics.Views;
            double ctlr = ctl * 100.00;
            
            embed.AddField("Latest Video",
                    $"[**{channel.LatestVideo.Title}**]({channel.LatestVideo.Url})\r\n"+ 
                    $"{channel.LatestVideo.Description.Limit(100)}")
                .AddField("Views", channel.LatestVideo.Statistics.FormattedViews, true)
                .AddField("Likes", channel.LatestVideo.Statistics.FormattedLikes + $" ({Math.Round(ctlr, 2)}% approx view-to-like ratio)", true)
                .AddField("Comments", channel.LatestVideo.Statistics.FormattedComments, true);
        } catch {}

        return embed.Build();
    }

    public async Task<Discord.Embed> GetPlaylistAsync(string playlistId)
    {
        var plyl = await GetPlaylistItemsAsync(playlistId);
        var plinf = await GetPlaylistInfoAsync(playlistId);
        var pllim = plyl.Take(15);

        var embed = new EmbedBuilder()
        {
            Title = plinf.Snippet.Title,
            Url = "https://youtube.com/playlist?list=" + plinf.Id,
            Description = plinf.Snippet.Description,
            Author = new EmbedAuthorBuilder()
            {
                Name = plinf.Snippet.ChannelTitle,
                Url = plinf.Snippet.ChannelId.IdUrl()
            }
        };

        foreach (var pli in pllim)
        {
            embed.AddField(pli.Snippet.Title, $"by [{pli.Snippet.VideoOwnerChannelTitle}]({pli.Snippet.VideoOwnerChannelId.IdUrl()})\r\n" + 
                                              $"**[Go to video](https://youtu.be/{pli.Snippet.ResourceId.VideoId})**\r\n" +
                                              pli.Snippet.Description.Limit(100), true);
        }

        return embed.Build();
    }

    public async Task<Discord.Embed> GetVideoAsync(string videoId)
    {
        var videos = await GetVideosAsync(videoId);
        var video = videos[0];
        
        double ctl = (double)video.Statistics.LikeCount / (double)video.Statistics.ViewCount;
        double ctlr = ctl * 100.00;

        EmbedBuilder embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = video.Snippet.ChannelTitle,
                    Url = video.Snippet.ChannelId.IdUrl()
                },
                Title = video.Snippet.Title,
                Url = $"https://youtu.be/{video.Id}",
                Description = video.Snippet.Description.Limit(100),
                ThumbnailUrl = video.Snippet.Thumbnails.Maxres.Url
            }
            .AddField("Views", video.Statistics.ViewCount.ToUlong().ToFormattedNumber(), true)
            .AddField("Likes", video.Statistics.LikeCount.ToUlong().ToFormattedNumber() + $" ({Math.Round(ctlr, 2)}% approx view-to-like ratio)", true);

        if (video.IsLiveStreamOngoing())
            embed.AddField("Concurrent Viewers", video.LiveStreamingDetails.ConcurrentViewers, true);
        else
            embed.AddField("Comments", video.Statistics.CommentCount.ToUlong().ToFormattedNumber(), true);

        return embed.Build();
    }
}