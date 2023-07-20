using System;
using Discord.Webhook;
using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;
using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json.Linq;

namespace SubscriberWebhook;

public class Program
{
    private static DiscordWebhookClient webhookClient;
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Subscriber Discord Webhook v1.0");
        
        webhookClient = new DiscordWebhookClient(AppConfig.discordWebhookToken);

        try
        {
            while (true)
            {
                var channelInfos = await Run();
                await PostToWebhook(channelInfos);
                await Task.Delay(AppConfig.updateInterval);
            }
        }
        catch (AggregateException ex)
        {
            foreach (var e in ex.InnerExceptions)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static async Task<List<ChannelInfo>> Run()
    {
        Console.WriteLine("Getting youtube subscribers to specified channel(s)...");
        
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = AppConfig.ytApiKey,
            ApplicationName = AppConfig.ytApiName
        });

        var channels = AppConfig.channels;

        List<ChannelInfo> channelInfos = new List<ChannelInfo>();

        var listRequest = youtubeService.Channels.List("snippet,statistics");
        listRequest.Id = channels;

        var channelResponse = await listRequest.ExecuteAsync();

        foreach (var c in channelResponse.Items)
        {
            ChannelInfo channelInfo = new ChannelInfo();
            
            channelInfo.SubscriberCount = (long?)c.Statistics.SubscriberCount ?? -1;

            EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder();
            embedFieldBuilder.Name = c.Snippet.Title;
            embedFieldBuilder.Value = String.Format("{0} subscribers", c.Statistics.SubscriberCount);
            embedFieldBuilder.IsInline = true;
            channelInfo.Field = embedFieldBuilder;

            channelInfos.Add(channelInfo);
        }

        return channelInfos;
    }

    private static ulong Message = AppConfig.webhookMessageIdOverride;

    private static async Task PostToWebhook(List<ChannelInfo> channelInfos)
    {
        Console.WriteLine("Got channels. Sending to Discord...");
        
        EmbedBuilder embedBuilder = new EmbedBuilder();
        embedBuilder.WithTitle("Channel Subscribers");
        embedBuilder.WithFields(channelInfos.OrderByDescending(a => a.SubscriberCount).Select(x => x.Field));
        embedBuilder.WithCurrentTimestamp();
        embedBuilder.WithFooter("Last updated");

        if (Message == 0)
            Message = await webhookClient.SendMessageAsync(embeds: new List<Embed> { embedBuilder.Build() },
                username: "YT Subscribers");
        else
            await webhookClient.ModifyMessageAsync(Message, x => x.Embeds = new List<Embed> { embedBuilder.Build() });
    }

    public static class AppConfig
    {
        private static string json = System.IO.File.ReadAllText("appsettings.json");
        private static JObject configObject = JObject.Parse(json);

        public static string ytApiKey = (string)configObject["YtApiKey"];
        public static string ytApiName = (string)configObject["YtApiName"];
        public static string discordWebhookToken = (string)configObject["DiscordWebhookToken"];
        public static string channels = (string)configObject["Channels"];
        public static int updateInterval = (int)configObject["UpdateInterval"] * 1000;
        public static ulong webhookMessageIdOverride = (ulong)configObject["WebhookMessageIdOverride"];
    }

    public class ChannelInfo
    {
        public long SubscriberCount;
        public EmbedFieldBuilder Field;
    }
}