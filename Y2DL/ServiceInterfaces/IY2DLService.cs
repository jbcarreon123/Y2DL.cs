using Y2DL.Models;

namespace Y2DL.ServiceInterfaces;

public interface IY2DLService<T>
{
    /// <summary>
    ///     Runs the Y2DL service, asynchronously.
    /// </summary>
    Task RunAsync(T data);
}