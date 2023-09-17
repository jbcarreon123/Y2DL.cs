using System.Diagnostics;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Y2DL.Attributes;

namespace Y2DL.Services.DiscordCommandsService;

[Group("about", "About this bot or Y2DL (if configured)")]
[DiscordCommands("y2dl.builtin.about")]
public class AboutCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("y2dl", "About Y2DL")]
    public async Task AboutY2DL()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        string version = fileVersionInfo.ProductVersion;

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle($"YouTube2DiscordLink [Y2DL] {version}")
            .WithDescription("Y2DL is a application that gets public info from YouTube's API and sends it to Discord.")
            .WithThumbnailUrl("https://jbcarreon123.github.io/Y2DL.png")
            .Build(), components: new ComponentBuilder()
            .WithButton("GitHub Repo", url: "https://github.com/jbcarreon123/y2dl", style: ButtonStyle.Link)
            .WithButton("Documentation", url: "https://jbcarreon123.github.io/docs/y2dl", style: ButtonStyle.Link)
            .Build());
    }
}