using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class DynamicChannelInfoService
{
    private static DatabaseManager _databaseManager = Program.Database;

    public static async Task Run(CancellationToken token, YoutubeService youtubeService)
    {
        while (!token.IsCancellationRequested)
        {
            foreach (var message in Program.WebhookClients.Item1)
            {
                var hash = Hashing.HashClassToSHA256String(message.Item1);

                if (await _databaseManager.Exists(hash, message.Item2.ChannelId))
                {
                    var chnl = await _databaseManager.Get(hash, message.Item2.ChannelId);

                    if (!message.Item1.Output.UseWebhook)
                    {
                        await Program.GetChannel(message.Item2.GuildId, message.Item2.ChannelId).ModifyMessageAsync(chnl, async m =>
                        {
                            m.Content = message.Item1.Content;
                            m.Embeds = new [] { message.Item1.Embed.ToDiscordEmbed(await youtubeService.GetChannel(message.Item1.ChannelId)) };
                        });
                    }
                    else
                    {
                        await message.Item2.Webhook.ModifyMessageAsync(chnl, async m =>
                        {
                            m.Content = message.Item1.Content;
                            m.Embeds = new [] { message.Item1.Embed.ToDiscordEmbed(await youtubeService.GetChannel(message.Item1.ChannelId)) };
                        });
                    }
                }
                else
                {
                    if (!message.Item1.Output.UseWebhook)
                    {
                        var msg = await Program.GetChannel(message.Item2.GuildId, message.Item2.ChannelId).SendMessageAsync(message.Item1.Content,
                            embed: message.Item1.Embed.ToDiscordEmbed(await youtubeService.GetChannel(message.Item1.ChannelId)));

                        await _databaseManager.Add(message.Item1.Output.ChannelId, new Database()
                        {
                            MessageHash = hash,
                            MessageId = msg.Id
                        });
                    }
                    else
                    {
                        var msg = await message.Item2.Webhook.SendMessageAsync(message.Item1.Content,
                            embeds: new List<Embed> { message.Item1.Embed.ToDiscordEmbed(await youtubeService.GetChannel(message.Item1.ChannelId)) },
                            username: Program.Config.Main.WebhookConfig.Name,
                            avatarUrl: Program.Config.Main.WebhookConfig.AvatarUrl);

                        await _databaseManager.Add(message.Item1.Output.ChannelId, new Database()
                        {
                            MessageHash = hash,
                            MessageId = msg
                        });
                    }
                }
            }
            
            await Task.Delay(Program.Config.Main.UpdateInterval, token);
        }
    }
}