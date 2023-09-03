using Discord;
using SmartFormat;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class LoopService
{
    public static bool CancelTask = false;
    
    public static async Task Run(YoutubeService youtubeService)
    {
        while (!CancelTask)
        {
            try
            {
                youtubeService.ResetChannels();
                
                if (Program.Config.Services.DynamicChannelInfo.Enabled && !CancelTask)
                {
                    // Dynamic Channel Info Service
                    foreach (var message in Program.WebhookClients.Item1)
                    {
                        var ytchnl = await youtubeService.GetChannel(message.Item1.ChannelId);
                        if (ytchnl is null) continue;
                        await DynamicChannelInfoService.Run(ytchnl, message);

                        await Task.Delay(Program.Config.Services.DynamicChannelInfo.LoopTimeout);
                    }
                }

                if (Program.Config.Services.ChannelReleases.Enabled && !CancelTask)
                {
                    // Channel Releases Service
                    foreach (var message in Program.WebhookClients.Item2)
                    {
                        var ytchnl = await youtubeService.GetChannel(message.Item1.ChannelId);
                        if (ytchnl is null) continue;
                        await ChannelReleasesService.Run(ytchnl, message);

                        await Task.Delay(Program.Config.Services.ChannelReleases.LoopTimeout);
                    }
                }

                if (Program.Config.Services.DynamicChannelInfoForVoiceChannels.Enabled && !CancelTask)
                {
                    // Dynamic Voice Channel Info Service
                    foreach (var voice in Program.VoiceChannels)
                    {
                        var ytchnl = await youtubeService.GetChannel(voice.Item3);
                        if (ytchnl is null) continue;
                        await DynamicVoiceChannelInfoService.Run(ytchnl, voice.Item2, voice.Item1.Name);

                        await Task.Delay(Program.Config.Services.DynamicChannelInfoForVoiceChannels.LoopTimeout);
                    }
                }
                
                if (Program.Config.Main.BotConfig.Status.Enabled && !CancelTask)
                {
                    var status = Program.Config.Main.BotConfig.Status.Status.Random().Emoji + " " ?? "";
                    status += Smart.Format(Program.Config.Main.BotConfig.Status.Status.Random().Text, new BotInfo()) ?? "";
                    await Program.GetCurrentUserClient().SetCustomStatusAsync(status);
                }
            }
            catch (Exception e)
            {
                await Program.Log(new LogMessage(LogSeverity.Warning, "Channels", "An error occured, skipping loop",
                    e));
            }

            await Task.Delay(Program.Config.Main.UpdateInterval);
        }
    }
}