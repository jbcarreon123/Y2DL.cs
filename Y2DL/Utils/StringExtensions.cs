using System.Text.RegularExpressions;

namespace Y2DL.Utils;

public static class StringExtensions
{
    public static bool IsNullOrWhitespace(this string? input)
    {
        return string.IsNullOrWhiteSpace(input);
    }

    public static string Limit(this string input, int maxLength)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        if (maxLength <= 0) throw new ArgumentException("maxLength must be greater than zero.");

        if (input.Length <= maxLength) return input;

        return input.Substring(0, maxLength - 3) + "...";
    }

    public static string GetYouTubeId(this string input)
    {
        var youtubeShortRegex = new Regex(@"youtu\.be/([a-zA-Z0-9_-]+)");
        var youtubeLongRegex = new Regex(@"v=([a-zA-Z0-9_-]+)");
        var playlistRegex = new Regex(@"list=([a-zA-Z0-9_-]+)");

        Match match;
        if ((match = youtubeShortRegex.Match(input)).Success) return match.Groups[1].Value;

        if ((match = youtubeLongRegex.Match(input)).Success) return match.Groups[1].Value;

        if ((match = playlistRegex.Match(input)).Success) return match.Groups[1].Value;

        return null;
    }
}