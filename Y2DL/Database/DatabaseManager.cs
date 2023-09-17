using System.Data;
using System.Data.SQLite;
using Serilog;
using Serilog.Events;
using Y2DL.Utils;

namespace Y2DL.Database;

public class DatabaseManager
{
    private static Y2dlDbContext _y2dlDbContext;

    public DatabaseManager(Y2dlDbContext Y2dlDbContext)
    {
        _y2dlDbContext = Y2dlDbContext;
    }

    public void Add(ulong channelId, ulong messageId, string youtubeChannelId)
    {
        _y2dlDbContext.Add(new DynamicChannelInfoMessages()
        {
            ChannelId = channelId,
            MessageId = messageId,
            YoutubeChannelId = youtubeChannelId,
            Hash = HashUtils.HashThingToSHA256String(channelId + messageId + youtubeChannelId)
        });
    }

    public ulong Get(string ytChannelId, ulong channelId)
    {
        var f = _y2dlDbContext.DynamicChannelInfoMessages
            .First(x => x.ChannelId == channelId && x.YoutubeChannelId == ytChannelId);

        return f.MessageId;
    }

    public bool Exists(string ytChannelId, ulong channelId)
    {
        try
        {
            _y2dlDbContext.DynamicChannelInfoMessages
                .First(x => x.ChannelId == channelId && x.YoutubeChannelId == ytChannelId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Remove(ulong messageId)
    {
        _y2dlDbContext.Remove(_y2dlDbContext.DynamicChannelInfoMessages.First(x => x.MessageId == messageId));
    }
}