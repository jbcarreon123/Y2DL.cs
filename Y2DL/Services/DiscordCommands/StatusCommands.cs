using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Y2DL.Attributes;
using Y2DL.Utils;

namespace Y2DL.Services.DiscordCommands;

public class StatusCommands : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("shards", "Show information for all shards")]
    public async Task Sharding()
    {
        var embed = new EmbedBuilder()
            .WithTitle($"Sharding")
            .AddField("Shards List",
                String.Join("\r\n",
                    Context.Client.Shards.Select(x =>
                        $"{(x.ConnectionState switch { ConnectionState.Connected => "🟩", ConnectionState.Connecting => "🟧", _ => "🟥" })} Shard **{x.ShardId}**")),
                true)
            .AddField("Guild Count",
                String.Join("\r\n",
                    Context.Client.Shards.Select(x => $"**{x.Guilds.Count}** guild{(x.Guilds.Count != 1 ? "s" : "")}")),
                true)
            .AddField("Ping", String.Join("\r\n", Context.Client.Shards.Select(x => $"**{x.Latency}**ms")), true);

        if (!Context.Interaction.IsDMInteraction)
        {
            embed.WithFooter(
                $"This guild is on Shard {Context.Client.GetShardIdFor(Context.Guild)} -- Guild ID is {Context.Guild.Id}");
        }
        else
        {
            embed.WithFooter("Oh wait, this is not a guild!");
        }
        
        await RespondAsync(embed: embed.Build());
    }
}