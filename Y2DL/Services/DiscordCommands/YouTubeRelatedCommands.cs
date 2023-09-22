using Discord;
using Discord.Interactions;
using Y2DL.Attributes;
using Y2DL.Utils;

namespace Y2DL.Services.DiscordCommands;

[Group("ytinfo", "Show YouTube channel/video/playlist info")]
public class YouTubeRelatedCommands : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly YoutubeService _youtubeService;
    
    public YouTubeRelatedCommands(YoutubeService youtubeService)
    {
        _youtubeService = youtubeService;
    }
    
    [SlashCommand("channel", "Show YouTube channel info")]
    public async Task YtChannel([Summary(description: "The Channel ID")] string channelId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetChannelAsync(channelId));
    }

    [SlashCommand("playlist", "Show YouTube playlist info")]
    public async Task YtPlaylist([Summary(description: "The Playlist ID")] string playlistId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetPlaylistAsync(playlistId));
    }

    [SlashCommand("video", "Show YouTube video info")]
    public async Task YtVideo([Summary(description: "The Video ID")] string videoId)
    {
        await DeferAsync();
        await FollowupAsync(embed: await _youtubeService.GetVideoAsync(videoId));
    }
}