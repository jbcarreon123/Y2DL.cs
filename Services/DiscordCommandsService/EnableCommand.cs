using Discord;
using Discord.Interactions;

namespace Y2DL.Services.DiscordCommandsService;

[Group("enable", "Enable service")]
[RequireUserPermission(ChannelPermission.ManageMessages)]
public class EnableCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ytservice", "Enable YouTube Service")]
    public async Task EnableYTService()
    {
        async void Action()
        {
            LoopService.CancelTask = false;
            await LoopService.Run(Program.GetYoutubeService());
        }

        var DataInThread = new Thread(Action)
        {
            IsBackground = true
        };
        DataInThread.Start();

        await RespondAsync("`YouTubeService` enabled", ephemeral: true);
    }
}