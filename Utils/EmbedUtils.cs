using System.Drawing;
using Discord;
using Y2DL.Models;
using SmartFormat;

namespace Y2DL.Utils;

public static class EmbedUtils
{
    public static EmbedBuilder ToDiscordEmbedBuilder(this Embeds embeds, YouTubeChannel channel)
    {
        try
        {
            var color = ColorTranslator.FromHtml(embeds.Color);

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = embeds.Author is not null ? Smart.Format(embeds.Author, channel) : "",
                    Url = embeds.AuthorUrl is not null ? Smart.Format(embeds.AuthorUrl, channel) : "",
                    IconUrl = embeds.AuthorAvatarUrl is not null ? Smart.Format(embeds.AuthorAvatarUrl, channel) : ""
                },
                Title = embeds.Title is not null ? Smart.Format(embeds.Title, channel) : "",
                Url = embeds.TitleUrl is not null ? Smart.Format(embeds.TitleUrl, channel) : "",
                Description = embeds.Description is not null ? Smart.Format(embeds.Description, channel) : "",
                Color = new Discord.Color(color.R, color.G, color.B),
                ImageUrl = embeds.ImageUrl is not null ? Smart.Format(embeds.ImageUrl, channel) : "",
                ThumbnailUrl = embeds.ThumbnailUrl is not null ? Smart.Format(embeds.ThumbnailUrl, channel) : "",
                Footer = new EmbedFooterBuilder()
                {
                    Text = embeds.Footer is not null ? Smart.Format(embeds.Footer, channel) : "",
                    IconUrl = embeds.FooterUrl is not null ? Smart.Format(embeds.FooterUrl, channel) : ""
                }
            };

            if (embeds.Fields is not null)
            {
                foreach (EmbedFields fields in embeds.Fields)
                {
                    embedBuilder.AddField(Smart.Format(fields.Name, channel), Smart.Format(fields.Value, channel),
                        fields.Inline);
                }
            }

            Program.Log(new LogMessage(LogSeverity.Debug, "Y2DL",
                $"Converted Embed to EmbedBuilder for YouTubeChannel {channel.Name}"));

            return embedBuilder;
        }
        catch (Exception e)
        {
            Program.Log(new LogMessage(LogSeverity.Error, "Formatting", "Cannot format message", e));

            return default;
        }
    }
    
    public static EmbedBuilder ToDiscordEmbedBuilder(this Embeds embeds, BotInfo botInfo)
    {
        try
        {
            var color = ColorTranslator.FromHtml(embeds.Color);

            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = embeds.Author is not null ? Smart.Format(embeds.Author, botInfo) : "",
                    Url = embeds.AuthorUrl is not null ? Smart.Format(embeds.AuthorUrl, botInfo) : "",
                    IconUrl = embeds.AuthorAvatarUrl is not null ? Smart.Format(embeds.AuthorAvatarUrl, botInfo) : ""
                },
                Title = embeds.Title is not null ? Smart.Format(embeds.Title, botInfo) : "",
                Url = embeds.TitleUrl is not null ? Smart.Format(embeds.TitleUrl, botInfo) : "",
                Description = embeds.Description is not null ? Smart.Format(embeds.Description, botInfo) : "",
                Color = new Discord.Color(color.R, color.G, color.B),
                ImageUrl = embeds.ImageUrl is not null ? Smart.Format(embeds.ImageUrl, botInfo) : "",
                ThumbnailUrl = embeds.ThumbnailUrl is not null ? Smart.Format(embeds.ThumbnailUrl, botInfo) : "",
                Footer = new EmbedFooterBuilder()
                {
                    Text = embeds.Footer is not null ? Smart.Format(embeds.Footer, botInfo) : "",
                    IconUrl = embeds.FooterUrl is not null ? Smart.Format(embeds.FooterUrl, botInfo) : ""
                }
            };

            if (embeds.Fields is not null)
            {
                foreach (EmbedFields fields in embeds.Fields)
                {
                    embedBuilder.AddField(Smart.Format(fields.Name, botInfo), Smart.Format(fields.Value, botInfo),
                        fields.Inline);
                }
            }

            Program.Log(new LogMessage(LogSeverity.Debug, "Y2DL",
                $"Converted Embed to EmbedBuilder for YouTubeChannel {botInfo.Name}"));

            return embedBuilder;
        }
        catch (Exception e)
        {
            Program.Log(new LogMessage(LogSeverity.Error, "Formatting", "Cannot format message", e));

            return default;
        }
    }
}