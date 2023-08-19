#nullable enable
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
    public string YoutubeApiKey { get; set; }
    public string YoutubeApiName { get; set; }
    public LogLevel LogLevel { get; set; }
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
    public string? Emoji { get; set; }
    public string? Text { get; set; }
}

public class Services
{
    public DynamicChannelInfo DynamicChannelInfo { get; set; }
    public ChannelReleases ChannelReleases { get; set; }
    public DynamicChannelInfoForVoiceChannels DynamicChannelInfoForVoiceChannels { get; set; }
    public Commands Commands { get; set; }
}

public class DynamicChannelInfo
{
    public bool Enabled { get; set; }
    public List<Message> Messages { get; set; }
}

public class ChannelReleases
{
    public bool Enabled { get; set; }
    public List<Message> Messages { get; set; }
}

public class DynamicChannelInfoForVoiceChannels
{
    public bool Enabled { get; set; }
    public List<Channels> Channels { get; set; }
}

public class Commands
{
    public bool Enabled { get; set; }
    public CommandType Type { get; set; }
    public string? Prefix { get; set; }
    public CommandList EnabledCommands { get; set; }
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
    public bool Inline { get; set; } = false;
}

public enum AppType
{
    [System.Runtime.Serialization.EnumMember(Value = @"Webhook")]
    Webhook,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Bot")]
    Bot
}

[Flags]
public enum CommandType
{
    [System.Runtime.Serialization.EnumMember(Value = @"Prefix")]
    Prefix,
    
    [System.Runtime.Serialization.EnumMember(Value = @"SlashCommand")]
    SlashCommand,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Both")]
    Both = Prefix | SlashCommand
}

[Flags]
public enum CommandList
{
    [System.Runtime.Serialization.EnumMember(Value = @"ChannelInfo")]
    ChannelInfo,
    [System.Runtime.Serialization.EnumMember(Value = @"LatestVideo")]
    LatestVideo
}

public enum BotState
{
    [System.Runtime.Serialization.EnumMember(Value = @"Offline")]
    Offline,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Online")]
    Online,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Idle")]
    Idle,
    
    [System.Runtime.Serialization.EnumMember(Value = @"AFK")]
    AFK,
    
    [System.Runtime.Serialization.EnumMember(Value = @"DoNotDisturb")]
    DoNotDisturb,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Invisible")]
    Invisible,
}

public enum LogLevel
{
    [System.Runtime.Serialization.EnumMember(Value = @"Critical")]
    Critical,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Error")]
    Error,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Warning")]
    Warning,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Info")]
    Info,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Verbose")]
    Verbose,
    
    [System.Runtime.Serialization.EnumMember(Value = @"Debug")]
    Debug,
}