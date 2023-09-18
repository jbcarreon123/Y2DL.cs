using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Y2DL.Attributes;

namespace Y2DL.Services.DiscordCommandsService;

[Group("about", "About this bot or Y2DL (if configured)")]
[DiscordCommands("y2dl.builtin.about")]
public class AboutCommands : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("y2dl", "About Y2DL")]
    public async Task AboutY2DL()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        var version = fileVersionInfo.ProductVersion;
        var vsl = fileVersionInfo.FileVersion;

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle($"YouTube2DiscordLink [Y2DL] {version}{(vsl is not ""? $" (in commit {vsl})": "")}")
            .WithDescription($"{(vsl is not ""? $"# WARNING\r\n[DVT-BLD2_cm1f3a25b]\r\nThis is a incomplete and is a pre-release.\r\nPlease see the updates on `HEAD` before writing an issue report.\r\n\r\n": "")}Y2DL is a application that gets public info from YouTube's API and sends it to Discord.")
            .WithThumbnailUrl("https://jbcarreon123.github.io/Y2DL.png")
            .AddField("Plugins", "This is a placeholder. `PluginManager` isn't implemented yet.\r\n**To check plugin's about, use /about plugin.**\r\n**Y2DL-Utils** by jbcarreon123\r\n**TwitchPlugin** by jbcarreon123\r\n**PubSubHubbubSupport** by jbcarreon123\r\n**OAuthThings** by jbcarreon123", true)
            .AddField("Thanks to:", "<@783601612601688074> and <@560123152349528066> for implementing it to their server,\r\n<@718620103830405181>, <@302305228416483331> and <@727630415808037008> for the inspiration,\r\nand **You** for using this bot.", true)
            .Build(), components: new ComponentBuilder()
            .WithButton("GitHub Repo", url: "https://github.com/jbcarreon123/y2dl", style: ButtonStyle.Link)
            .WithButton("Documentation", url: "https://jbcarreon123.github.io/docs/y2dl", style: ButtonStyle.Link)
            .Build());
    }
}