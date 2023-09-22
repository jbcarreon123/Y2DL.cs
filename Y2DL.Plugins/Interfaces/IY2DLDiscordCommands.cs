using Discord.Interactions;

namespace Y2DL.Plugins.Interfaces;

public interface IY2DLDiscordCommands
{
    void ConfigureSlashCommands(ShardedInteractionContext context);
}