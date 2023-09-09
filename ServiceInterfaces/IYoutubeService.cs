using Y2DL.Models;
using Y2DL.Services;

namespace Y2DL.ServiceInterfaces;

/// <summary>
/// The interface of <see cref="YoutubeService"/>
/// </summary>
public interface IYoutubeService
{
    /// <summary>
    /// Gets the specified YouTube channel, asynchronously.
    /// </summary>
    /// <param name="channelId">the ChannelId of the youtube channel.</param>
    /// <returns>a <see cref="YoutubeChannel" /> for identifying the channel.</returns>
    Task<YoutubeChannel> GetChannelAsync(string channelId);
}