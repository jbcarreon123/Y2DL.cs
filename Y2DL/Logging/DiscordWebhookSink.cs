using Discord;
using Discord.Webhook;
using Serilog.Core;
using Serilog.Events;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Logging;

public class DiscordWebhookSink : ILogEventSink
{
    private readonly Config _config;

    public DiscordWebhookSink(Config config)
    {
        _config = config;
    }

    public void Emit(LogEvent logEvent)
    {
        var webhookClient = new DiscordWebhookClient(_config.Main.Logging.LogErrorChannel.WebhookUrl);

        webhookClient.SendMessageAsync(embeds: new[]
            {
                new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = logEvent.Level.ToString()
                    },
                    Title = logEvent.MessageTemplate.Text,
                    Description = logEvent.Exception is not null
                        ? logEvent.Exception.ToString().Limit(250)
                        : ""
                }.Build()
            }
        );
    }
}