using YamlDotNet;
using YamlDotNet.Serialization;

namespace Y2DL.Config;

public class Config
{
    public AppType Type { get; set; }
    public string DiscordToken { get; set; }
    public string YoutubeApiKey { get; set; }
    public string YoutubeApiName { get; set; }
}

public class Commands
{
    public string Prefix { get; set; }
    public CommandType Type { get; set; }
    public CommandList EnabledCommands { get; set; }
}

public class ChannelInfoMessage
{
    public string? Content { get; set; }
    public List<Embeds> Embeds { get; set; }
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
    Basic,
    Simple,
    Verbose,
    Bot
}

[Flags]
public enum CommandType
{
    Prefix,
    SlashCommand,
    Both
}

[Flags]
public enum CommandList
{
    ChannelInfo,
    LatestVideo,
    Configure,
    Ping
}