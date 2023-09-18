﻿using System.ServiceModel.Syndication;
using System.Xml;
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
                Log.Error(e, "Google API thrown an exception, possibly API quota exceeded");
                return null;
            }

            youtubeService = _youTubeServices.Next(youtubeService);
            _youTubeServices.MoveFirstToLast();
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
                var video = await GetLatestVideoForChannels(channelIds, youtubeService);
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
                    }
                });
            }

            return youtubeChannels;
        }
        catch (Exception e)
        {
            Log.Warning(e, "An error occured while getting channels");
            
            return null;
        }
    }

    private async Task<Listable<PlaylistItem>> GetPlaylistItemsAsync(Listable<string> channelIds, YouTubeService youTubeService)
    {
        Listable<PlaylistItem> playlistItems = new Listable<PlaylistItem>();
        
        using (var httpClient = new HttpClient())
        {
            foreach (var channelId in channelIds)
            {
                if (channelId.IsNullOrWhitespace())
                    continue;
                
                try
                {
                    var playlistItemsRequest = youTubeService.PlaylistItems.List("snippet,contentDetails");
                    playlistItemsRequest.PlaylistId = "UULF" + channelId.Substring(2);
                    var playlistItemsResponse = await playlistItemsRequest.ExecuteAsync();
                    playlistItems.AddRange(playlistItemsResponse.Items.Take(1));
                } catch {}
            }
        }

        return playlistItems;
    }
    
    private async Task<List<Video>> GetLatestVideoForChannels(Listable<string> channelIds, YouTubeService youtubeService)
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
        var plItems = await GetPlaylistItemsAsync(channelIds, youtubeService);
        videoIds.AddRange(plItems.Select(x => x.ContentDetails.VideoId));
        
        var vidListRequest =
            youtubeService.Videos.List("snippet,statistics,contentDetails,liveStreamingDetails");
        vidListRequest.Id = videoIds;
        var videoResponse = await vidListRequest.ExecuteAsync();
        return videoResponse.Items.ToList();
    }
}