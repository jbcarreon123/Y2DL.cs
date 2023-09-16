using Y2DL.Models;
using Y2DL.Services;

namespace Y2DL.ServiceInterfaces;

/// <summary>
/// The interface of <see cref="Y2DL.Services.YoutubeService"/>
/// </summary>
public interface IYoutubeService
{
    /// <summary>
    /// Gets the specified YouTube channel, asynchronously.
    /// </summary>
    /// <param name="channelIds">a channel ID or list of channel IDs to identify each YouTube channel.</param>
    /// <returns>a <see cref="YoutubeChannel" /> for identifying the channel or channels.</returns>
    Task<Listable<YoutubeChannel>> GetChannelsAsync(Listable<string> channelIds);
}