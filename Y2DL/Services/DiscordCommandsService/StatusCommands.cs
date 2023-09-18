using System.Diagnostics;
using Discord;
using Discord.Interactions;
using Y2DL.Attributes;
using Y2DL.Utils;

namespace Y2DL.Services.DiscordCommandsService;

[DiscordCommands("y2dl.builtin.status")]
public class StatusCommands : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("status", "Status of the bot")]
    public async Task Status()
    {
        var emojis = "";
        foreach (var shard in Context.Client.Shards)
        {
            if (shard.Latency != 0)
                emojis += "🟩";
            else
                emojis += "🟥";
        }

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle($"Bot Status")
            .AddField("Shards", emojis)
            .Build());
    }
    
    [SlashCommand("ping", "Ping the bot!")]
    public async Task Ping()
    {
        var stopwatch = Stopwatch.StartNew();
        
        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle($"Ping the bot")
            .AddField("Heartbeat", $"Getting it, please wait.", true)
            .AddField("Init Latency", "Getting it, please wait.", true)
            .Build());

        var msg = "";

        foreach (var shard in Context.Client.Shards)
        {
            msg += $"Shard {shard.ShardId}: **{shard.Latency}**ms\r\n";
        }
        
        stopwatch.Stop();
        
        await ModifyOriginalResponseAsync(x => x.Embed = new EmbedBuilder()
            .WithTitle($"Ping the bot")
            .AddField("Heartbeat", msg, true)
            .AddField("Init Latency", $"**{stopwatch.Elapsed.TotalMilliseconds.Round()}**ms", true)
            .Build());
    }
}