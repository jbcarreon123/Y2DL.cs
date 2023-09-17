using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Y2DL.Models;

namespace Y2DL.Logging;

public static class DiscordWebhookSinkExtensions
{
    public static LoggerConfiguration Discord(
        this LoggerSinkConfiguration loggerConfiguration,
        Config config,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning)
    {
        return loggerConfiguration.Sink(
            new DiscordWebhookSink(config),
            restrictedToMinimumLevel);
    }
}