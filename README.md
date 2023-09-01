<p align="center"><img src="/Images/y2dl-flat.png" height="100" /><br />
<a href="https://jbcarreon123.github.io/docs/y2dl/faq">FAQ</a></p>

# YouTube2DiscordLink (formerly YTSCTD)
Gets channel info from one or multiple channels, and sends it on a Text channel or on a Voice channel.
**Note that this is NOT a YouTube Downloader.**

## Screenshot (DynamicVoiceChannelInfo service, and /about y2dl)
![image](https://github.com/jbcarreon123/Y2DL/assets/86447165/dafd454e-3545-4306-8433-7349a8770980)

## Currently in beta!
But, if you want to see updates, and have a idea for a feature of Y2DL,
you can go to my discord at https://discord.gg/P5ecFZNyCc, in the #y2dl channel.

## Config file
```yaml
# YouTube2DiscordLink (formerly YTSCTD) 2.0.0
# Config File
# For smooth operation, follow the instructions at https://jbcarreon123.github.io/docs/y2dl

# DO NOT TOUCH THIS!
Version: 2.0.0-b1

Main:
  # Webhook or Bot
  Type: Bot

  # If bot, configure this.
  # If not, don't touch this.
  BotConfig: 
    BotToken: [YOUR BOT TOKEN HERE]
    State: DoNotDisturb
    Status:
      Enabled: true
      Emoji: 🔗
      Text: jbcarreon123.github.io/y2dl

  WebhookConfig:
    Name: Y2DL Relay Webhook
    AvatarUrl: https://jbcarreon123.github.io/Y2DL.png

  # The update interval (in milliseconds)
  UpdateInterval: 60000

  # Required at least 1 so it can get the info.
  ApiKeys:
    - YoutubeApiKey: [YOUR API KEY HERE]
      YoutubeApiName: [YOUR API NAME HERE]

  # Defines the log severity to shown in the console.
  Logging:
    LogLevel: Verbose
    LogErrorChannel:
      UseWebhook: false
      WebhookUrl: ""
      GuildId: 0
      ChannelId: 0

Services:
  DynamicChannelInfo:
    Enabled: false
    # Supports the full Discord Markdown (for Bots/Webhooks).
    # {channel} for Channel Name, and {channelUrl} for channel's URL
    # {lVidName} for latest video's title, {lVidThumbnail} for it's thumbnail,
    # and {lVidUrl} for it's URL
    Messages:
      - ChannelId: [CHANNEL ID HERE]
        Output: 
          # If you want to use a webhook instead of a bot, without using Webhook mode, set this to true.
          # Note: Only used in Bot mode. If in Webhook mode, it is ignored because you already using a webhook.
          UseWebhook: false
          # Only used in Webhook mode, or if you set UseWebhook to true.
          WebhookUrl: [YOUR DISCORD WEBHOOK URL]
		  # Used in both
          ChannelId: 0
          # The thing below is only used in Bot mode.
          GuildId: 0
          
        Embed:
          Title: "{Name} ({Handle})"
          TitleUrl: "{ChannelUrl}"
          Description: |
            {Description}
            Subscribers: **{Statistics.Subscribers}**
          Color: "#552233"
          ImageUrl: "{LatestVideo.Thumbnail}"
          ThumbnailUrl: "{ChannelAvatarUrl}"
          Fields:
            - Name: "Latest video"
              Value: |
                [{LatestVideo.Title}]({LatestVideo.Url})
                Views: {LatestVideo.Statistics.Views}
                Likes: {LatestVideo.Statistics.Likes}
                Comments: {LatestVideo.Statistics.Comments}

  ChannelReleases:
    Enabled: true
    # Supports the full Discord Markdown (for Bots/Webhooks).
    # {channel} for Channel Name, and {channelUrl} for channel's URL
    # {lVidName} for latest video's title, {lVidThumbnail} for it's thumbnail,
    # and {lVidUrl} for it's URL
    # Go to https://jbcarreon123.github.io/docs/y2dl/message for more variables.
    Messages:
      - ChannelId: [CHANNEL ID HERE]
        Output: 
          # If you want to use a webhook instead of a bot, without using Webhook mode, set this to true.
          UseWebhook: false
          # Only used in Webhook mode, or if you set UseWebhook to true.
          WebhookUrl: [YOUR DISCORD WEBHOOK URL]
          # Used in both
          ChannelId: 0
          # The 2 things below is only used in Bot mode.
          GuildId: 0
        Content: "@everyone"
        Embed:
          Author: "New video on {Name} is just released!"
          Title: "{LatestVideo.Title}"
          TitleUrl: "{LatestVideo.Url}"
          Description: "{LatestVideo.Description}"
          Color: "#252525"
          ImageUrl: "{LatestVideo.Thumbnail}"

  # Only works if Type is Bot. Else, it will be ignored.
  DynamicChannelInfoForVoiceChannels:  
    Enabled: true
    # {channel} for Channel Name, and {channelSubscribers} for subscibers.
    # Go to https://jbcarreon123.github.io/docs/y2dl/voicechannels for more variables.
    Channels:
      - ChannelId: [CHANNEL ID HERE]
        VoiceChannels:
          - GuildId: 0
            ChannelId: 0
            Name: "Subscribers: {Statistics.FormattedSubscribers}"

  # Only works if Type is Bot. Else, it will be ignored.
  Commands:
    Enabled: true
    # Available types: SlashCommand, Prefix, or Both.
    Type: Both
    # Only used if Type is Prefix or Both.
    Prefix: "!"
    About:
      Enabled: true
      Embed:
        Author: "{Name}"
        AuthorAvatarUrl: "{AvatarUrl}"
        Fields:
          - Name: "Just a bot."
            Value: "Yeah."
```

## Disclaimer
YouTube is a trademark by Google Inc., and  
Discord is a trademark by Discord Inc.  
This program isn't endorsed or affiliated by Google or Discord.
