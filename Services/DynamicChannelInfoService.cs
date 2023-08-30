using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class DynamicChannelInfoService
{
    public static List<(ulong, IUserMessage)> BotDynamicMessages { get; set; } = new();
    private static DatabaseManager _databaseManager = Program.Database;

    public static async Task Run(YouTubeChannel channel, (Message, Program.Output) message)
    {
        var hash = Hashing.HashClassToSHA256String(message.Item1);

        if (await _databaseManager.Exists(hash, message.Item2.ChannelId))
        {
            var chnl = await _databaseManager.Get(hash, message.Item2.ChannelId);

            if (!message.Item1.Output.UseWebhook)
            {
                var msg = await Program.GetTextChannel(message.Item2.GuildId, message.Item2.ChannelId)
                    .ModifyMessageAsync(chnl, m =>
                    {
                        m.Content = message.Item1.Content;
                        m.Embeds = new[]
                        {
                            message.Item1.Embed.ToDiscordEmbedBuilder(channel).Build()
                        };
                    });
            }
            else
            {
                await message.Item2.Webhook.ModifyMessageAsync(chnl, m =>
                {
                    m.Content = message.Item1.Content;
                    m.Embeds = new[]
                    {
                        message.Item1.Embed.ToDiscordEmbedBuilder(channel).Build()
                    };
                });
            }
        }
        else
        {
            if (!message.Item1.Output.UseWebhook)
            {
                var msg = await Program.GetTextChannel(message.Item2.GuildId, message.Item2.ChannelId).SendMessageAsync(
                    message.Item1.Content,
                    embed: message.Item1.Embed
                        .ToDiscordEmbedBuilder(channel).Build());

                await _databaseManager.Add(message.Item1.Output.ChannelId, new Database()
                {
                    MessageHash = hash,
                    MessageId = msg.Id
                });

                BotDynamicMessages.Add((msg.Id, msg));
            }
            else
            {
                var msg = await message.Item2.Webhook.SendMessageAsync(message.Item1.Content,
                    embeds: new List<Discord.Embed>
                    {
                        message.Item1.Embed
                            .ToDiscordEmbedBuilder(channel).Build()
                    },
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
}