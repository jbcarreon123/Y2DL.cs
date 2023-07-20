# YoutubeSubCountToDiscord
Get subscriber count of one or multiple channels, and outputs it as a Discord Webhook.

## Default config
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <appSettings>
        <!-- Your YouTube API key -->
        <!-- For security purposes, I won't add my API key here. -->
        <!-- https://developers.google.com/youtube/v3/getting-started -->
        <add key="YtApiKey" value="[INSERT API KEY HERE]" />

	<!-- The application name you specified on Google Developer Console. -->
        <add key="YtApiName" value="[INSERT APP NAME HERE]" />
        
        <!-- Your Discord webhook URL. The name of the bot does not matter. -->
        <!-- https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks -->
        <add key="DiscordWebhookToken" value="[INSERT WEBHOOK URL HERE]" />
        
        <!-- What channels you want to get the subscriber count? Use channel IDs. -->
	<!-- Accepts multiple channel IDs using a comma-seperated list. -->
        <!-- https://support.google.com/youtube/answer/3250431?hl=en -->
	<!-- This also accepts channels that you don't own. -->
        <add key="Channels" value="[CHANNEL 1 ID HERE],[CHANNEL 2 ID HERE]" />
        
        <!-- What is the delay of updating the channels? Default: 300 seconds (5 minutes) -->
        <!-- Accepts seconds. 1 minute = 60 seconds -->
	<!-- Please note that YouTube API has a quota limit of 10000 per day, and each request of this program costs 1 quota. -->
        <add key="UpdateInterval" value="300" />
        
        <!-- If you want the webhook to not create a new message for every reboot, add a message ID here. Default: 0 (disabled) -->
        <!-- To make it work and make sure the program won't crash, use a ID of a message that the webhook created -->
        <!-- To get message ID, enable Developer Mode on advanced settings in Discord, right click to the webhook's message, and click Copy Message ID. -->
        <!-- https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID- -->
        <add key="WebhookMessageIdOverride" value="0" />
    </appSettings>
</configuration>
```

## Depedencies
Google.Apis.Youtube.v3
Discord.Net
