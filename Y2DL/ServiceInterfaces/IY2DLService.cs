using Y2DL.Models;

namespace Y2DL.ServiceInterfaces;

public interface IY2DLService
{
    /// <summary>
    ///     Runs the Y2DL service, asynchronously.
    /// </summary>
    /// <param name="youtubeChannel">The <see cref="YoutubeChannel"/> for the specified channel.</param>
    Task RunAsync(YoutubeChannel youtubeChannel);
}