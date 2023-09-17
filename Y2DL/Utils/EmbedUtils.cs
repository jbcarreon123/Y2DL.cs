using System.Drawing;
using Discord;
using Serilog;
using Y2DL.Models;
using SmartFormat;

namespace Y2DL.Utils;

public static class EmbedUtils
{
    public static EmbedBuilder ToDiscordEmbedBuilder(this Embeds embeds, YoutubeChannel channel)
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

            Log.Debug($"Y2DL: Converted Embed to EmbedBuilder for YouTubeChannel {channel.Name}");

            return embedBuilder;
        }
        catch (Exception e)
        {
            Log.Warning(e, $"An error occured while converting");

            return default;
        }
    }
}