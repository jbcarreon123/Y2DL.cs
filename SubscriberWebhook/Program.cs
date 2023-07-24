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
        

        var listRequest = youtubeService.Channels.List("snippet,statistics,contentDetails");
        listRequest.Id = channels;

        var channelResponse = await listRequest.ExecuteAsync();

        var plListRequest = youtubeService.PlaylistItems.List("snippet,contentDetails");

        var vidListRequest = youtubeService.Videos.List("snippet,statistics,contentDetails");

        foreach (var c in channelResponse.Items)
        {
            plListRequest.PlaylistId = c.ContentDetails.RelatedPlaylists.Uploads;
            plListRequest.MaxResults = 1;
            var playlist = await plListRequest.ExecuteAsync();
            var pli = playlist.Items[0];

            vidListRequest.Id = pli.Snippet.ResourceId.VideoId;
            var video = await vidListRequest.ExecuteAsync();
            var vid = video.Items[0];
            
            ChannelInfo channelInfo = new ChannelInfo();
            
            channelInfo.SubscriberCount = (long?)c.Statistics.SubscriberCount ?? -1;

            if (AppConfig.Mode == EmbedMode.SIMPLE || AppConfig.Mode == EmbedMode.VERBOSE)
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle(c.Snippet.Title);
                builder.WithUrl(String.IsNullOrWhiteSpace(c.Snippet.CustomUrl)
                    ? $"https://youtube.com/channels/{c.Id}"
                    : $"https://youtube.com/{c.Snippet.CustomUrl}");
                builder.AddField("Channel Info",
                    $"{c.Statistics.SubscriberCount} subscribers,\r\n{c.Statistics.VideoCount} videos,\r\n{c.Statistics.ViewCount} views");
                builder.AddField("Latest Video",
                    $"[{pli.Snippet.Title}](https://youtu.be/{pli.Snippet.ResourceId.VideoId})\r\nReleased <t:{vid.Snippet.PublishedAtDateTimeOffset.Value.ToUnixTimeSeconds()}>\r\n{vid.Statistics.ViewCount} views,\r\n{vid.Statistics.LikeCount} likes");
                builder.WithCurrentTimestamp();
                builder.WithFooter("Last updated");

                if (AppConfig.Mode == EmbedMode.VERBOSE)
                {
                    builder.WithThumbnailUrl(c.Snippet.Thumbnails.High.Url);
                    builder.WithImageUrl(vid.Snippet.Thumbnails.Maxres.Url);
                    builder.WithDescription(c.Snippet.Description);
                }

                channelInfo.Embed = builder.Build();
            }
            else
            {
                EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder();
                embedFieldBuilder.Name = c.Snippet.Title;
                embedFieldBuilder.Value = String.Format("{0} subscribers", c.Statistics.SubscriberCount);
                embedFieldBuilder.IsInline = true;
                channelInfo.Field = embedFieldBuilder;
            }

            channelInfos.Add(channelInfo);
        }

        return channelInfos;
    }

    private static ulong Message = AppConfig.webhookMessageIdOverride;

    private static async Task PostToWebhook(List<ChannelInfo> channelInfos)
    {
        Console.WriteLine("Got channels. Sending to Discord...");

        if (AppConfig.Mode == EmbedMode.SIMPLE || AppConfig.Mode == EmbedMode.VERBOSE)
        {
            if (Message == 0)
                Message = await webhookClient.SendMessageAsync(embeds: channelInfos.Select(x => x.Embed),
                    username: "YTSCTD 1.2.0 (by jbcarreon123)");
            else
                await webhookClient.ModifyMessageAsync(Message,
                    x => x.Embeds = channelInfos.Select(x => x.Embed).ToArray());
        }
        else
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle("Channel Subscribers");
            embedBuilder.WithFields(channelInfos.OrderByDescending(a => a.SubscriberCount).Select(x => x.Field));
            embedBuilder.WithCurrentTimestamp();
            embedBuilder.WithFooter("Last updated");

            if (Message == 0)
                Message = await webhookClient.SendMessageAsync(embeds: new List<Embed> { embedBuilder.Build() },
                    username: "YTSCTD 1.2.0 (by jbcarreon123)");
            else
                await webhookClient.ModifyMessageAsync(Message, x => x.Embeds = new List<Embed> { embedBuilder.Build() });
        }

        if (AppConfig.UpdateCSVFile)
            ToCsv(channelInfos, AppConfig.CSVFileDestination);
    }

    public static bool NeedsToBeAppended = !AppConfig.ForceAppendCSV;

    public static void ToCsv(List<ChannelInfo> Data, string CsvPath)
    {
        Console.WriteLine("Writing to the CSV file...");
        
        if (NeedsToBeAppended)
        {
            try
            {
                File.WriteAllText(CsvPath,
                    $"Date,{String.Join(",", Data.OrderByDescending(a => a.SubscriberCount).Select(x => x.Embed.Title))}\r\n");
            }
            catch
            {
                File.WriteAllText(CsvPath,
                    $"Date,{String.Join(",", Data.OrderByDescending(a => a.SubscriberCount).Select(x => x.Field.Name))}\r\n");
            }

            NeedsToBeAppended = false;
        }

        try
        {
            File.AppendAllText(CsvPath,
                $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},{String.Join(",", Data.OrderByDescending(a => a.SubscriberCount).Select(x => x.SubscriberCount))}\r\n");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("An error occured while writing. {0}", e.Message);
        }
    }

    public static class AppConfig
    {
        private static string json = System.IO.File.ReadAllText("appsettings.json");
        private static JObject configObject = JObject.Parse(json);

        public static EmbedMode Mode = Enum.Parse<EmbedMode>((string)configObject["Mode"]);
        public static string ytApiKey = (string)configObject["YtApiKey"];
        public static string ytApiName = (string)configObject["YtApiName"];
        public static string discordWebhookToken = (string)configObject["DiscordWebhookToken"];
        public static string channels = (string)configObject["Channels"];
        public static int updateInterval = Convert.ToInt32(configObject["UpdateInterval"]) * 1000;
        public static ulong webhookMessageIdOverride = Convert.ToUInt64(configObject["WebhookMessageIdOverride"]);
        public static bool UpdateCSVFile = !String.IsNullOrWhiteSpace((string)configObject["CSVFileDestination"]);
        public static string CSVFileDestination = (string)configObject["CSVFileDestination"];
        public static bool ForceAppendCSV = (bool)configObject["ForceAppendCSV"];
    }

    public enum EmbedMode
    {
        BASIC,
        SIMPLE,
        VERBOSE
    }

    public class ChannelInfo
    {
        public long SubscriberCount;
        public Embed Embed;
        public EmbedFieldBuilder Field;
    }
}