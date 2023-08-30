using Discord;
using Y2DL.Models;

namespace Y2DL.Services;

public class LoopService
{
    public static async Task Run(CancellationToken token, YoutubeService youtubeService)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (Program.Config.Services.DynamicChannelInfo.Enabled)
                {
                    // Dynamic Channel Info Service
                    foreach (var message in Program.WebhookClients.Item1)
                    {
                        var ytchnl = await youtubeService.GetChannel(message.Item1.ChannelId);
                        if (ytchnl is null) continue;
                        await DynamicChannelInfoService.Run(ytchnl, message);

                        await Task.Delay(Program.Config.Services.DynamicChannelInfo.LoopTimeout, token);
                    }
                }

                if (Program.Config.Services.ChannelReleases.Enabled)
                {
                    // Channel Releases Service
                    foreach (var message in Program.WebhookClients.Item2)
                    {
                        var ytchnl = await youtubeService.GetChannel(message.Item1.ChannelId);
                        if (ytchnl is null) continue;
                        await ChannelReleasesService.Run(ytchnl, message);

                        await Task.Delay(Program.Config.Services.ChannelReleases.LoopTimeout, token);
                    }
                }

                if (Program.Config.Services.DynamicChannelInfoForVoiceChannels.Enabled)
                {
                    // Channel Releases Service
                    foreach (var voice in Program.VoiceChannels)
                    {
                        var ytchnl = await youtubeService.GetMinimalChannel(voice.Item3);
                        if (ytchnl is null) continue;
                        await DynamicVoiceChannelInfoService.Run(ytchnl, voice.Item2, voice.Item1.Name);

                        await Task.Delay(Program.Config.Services.DynamicChannelInfoForVoiceChannels.LoopTimeout, token);
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(new LogMessage(LogSeverity.Warning, "Channels", "An error occured, skipping loop",
                    e));
            }

            await Task.Delay(Program.Config.Main.UpdateInterval, token);
        }
    }
}