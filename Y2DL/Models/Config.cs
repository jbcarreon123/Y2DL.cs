using System.Runtime.Serialization;

namespace Y2DL.Models;

public class Config
{
    public string Version { get; set; }
    public Main Main { get; set; }
    public Services Services { get; set; }
}

public class Main
{
    public AppType Type { get; set; }
    public BotConfig BotConfig { get; set; }
    public WebhookConfig WebhookConfig { get; set; }
    public int UpdateInterval { get; set; }
    public List<ApiKeys> ApiKeys { get; set; }
    public Logging Logging { get; set; }
}

public class Logging
{
    public LogLevel LogLevel { get; set; }
    public Output LogErrorChannel { get; set; }
}

public class ApiKeys
{
    public string YoutubeApiKey { get; set; }
    public string YoutubeApiName { get; set; }
}

public class WebhookConfig
{
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
}

public class BotConfig
{
    public string BotToken { get; set; }
    public BotState State { get; set; }
    public BotStatus Status { get; set; }
}

public class BotStatus
{
    public bool Enabled { get; set; }
    public List<BotCustomStatus>? Status { get; set; }
}

public class BotCustomStatus
{
    public string Emoji { get; set; }
    public string Text { get; set; }
}

public class Services
{
    public DynamicChannelInfoConfig DynamicChannelInfo { get; set; }
    public ChannelReleases ChannelReleases { get; set; }
    public DynamicChannelInfoForVoiceChannels DynamicChannelInfoForVoiceChannels { get; set; }
    public Commands Commands { get; set; }
}

public class LinkedSubRoles
{
    public bool Enabled { get; set; }
    public bool AlsoRemoveRoles { get; set; }

    public List<LinkedChannels> Channels { get; set; }
}

public class LinkedChannels
{
    public string ChannelId;
    public ulong RoleId;
}

public class Embed
{
    public string? Author { get; set; }
    public string? AuthorUrl { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string? Title { get; set; }
    public string? TitleUrl { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Footer { get; set; }
    public string? FooterUrl { get; set; }

    public EmbedFields Field { get; set; } = new()
    {
        Inline = false,
        Name = "{Channel.Name}",
        Value = "{Role.Mention}"
    };
}

public class DynamicChannelInfoConfig
{
    public bool Enabled { get; set; }
    public List<Message> Messages { get; set; }
    public int LoopTimeout { get; set; } = 1000;
}

public class ChannelReleases
{
    public bool Enabled { get; set; }
    public List<Message> Messages { get; set; }
    public int LoopTimeout { get; set; } = 1000;
}

public class DynamicChannelInfoForVoiceChannels
{
    public bool Enabled { get; set; }
    public List<Channels> Channels { get; set; }
    public int LoopTimeout { get; set; } = 1000;
}

public class Commands
{
    public bool Enabled { get; set; }
    public CommandType Type { get; set; }
    public string? Prefix { get; set; }
    public About About { get; set; }
}

public class About
{
    public bool Enabled { get; set; }
    public Embeds Embed { get; set; }
}

public class Channels
{
    public string ChannelId { get; set; }
    public List<VoiceChannels> VoiceChannels { get; set; }
}

public class VoiceChannels
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string Name { get; set; }
}

public class Message
{
    public string ChannelId { get; set; }
    public Output Output { get; set; }
    public string Content { get; set; }
    public Embeds Embed { get; set; }
}

public class Output
{
    public bool UseWebhook { get; set; }
    public string WebhookUrl { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
}

public class Embeds
{
    public string? Author { get; set; }
    public string? AuthorUrl { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public string? Title { get; set; }
    public string? TitleUrl { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Footer { get; set; }
    public string? FooterUrl { get; set; }
    public List<EmbedFields>? Fields { get; set; }
}

public class EmbedFields
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool Inline { get; set; }
}

public enum AppType
{
    [EnumMember(Value = @"Webhook")] Webhook,

    [EnumMember(Value = @"Bot")] Bot
}

[Flags]
public enum CommandType
{
    [EnumMember(Value = @"Prefix")] Prefix,

    [EnumMember(Value = @"SlashCommand")] SlashCommand,

    [EnumMember(Value = @"Both")] Both = Prefix | SlashCommand
}

[Flags]
public enum CommandList
{
    [EnumMember(Value = @"ChannelInfo")] ChannelInfo,
    [EnumMember(Value = @"LatestVideo")] LatestVideo
}

public enum BotState
{
    [EnumMember(Value = @"Offline")] Offline,

    [EnumMember(Value = @"Online")] Online,

    [EnumMember(Value = @"Idle")] Idle,

    [EnumMember(Value = @"AFK")] AFK,

    [EnumMember(Value = @"DoNotDisturb")] DoNotDisturb,

    [EnumMember(Value = @"Invisible")] Invisible
}

public enum LogLevel
{
    [EnumMember(Value = @"Critical")] Critical,

    [EnumMember(Value = @"Error")] Error,

    [EnumMember(Value = @"Warning")] Warning,

    [EnumMember(Value = @"Info")] Info,

    [EnumMember(Value = @"Verbose")] Verbose,

    [EnumMember(Value = @"Debug")] Debug
}