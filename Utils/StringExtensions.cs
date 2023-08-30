using System.Text.RegularExpressions;

namespace Y2DL.Utils;

public static class StringExtensions
{
    public static string GetYouTubeId(this string input)
    {
        // Define regular expressions for both youtu.be and youtube.com URLs
        Regex youtubeShortRegex = new Regex(@"youtu\.be/([a-zA-Z0-9_-]+)");
        Regex youtubeLongRegex = new Regex(@"v=([a-zA-Z0-9_-]+)");
        Regex playlistRegex = new Regex(@"list=([a-zA-Z0-9_-]+)");

        // Try to match against each regular expression
        Match match;
        if ((match = youtubeShortRegex.Match(input)).Success)
        {
            // If it's a short URL (youtu.be), return the captured group
            return match.Groups[1].Value;
        }
        else if ((match = youtubeLongRegex.Match(input)).Success)
        {
            // If it's a long URL (youtube.com), return the captured group
            return match.Groups[1].Value;
        }
        else if ((match = playlistRegex.Match(input)).Success)
        {
            // If it's a playlist URL, return the captured group
            return match.Groups[1].Value;
        }
        else
        {
            // If no match is found, return null or throw an exception as desired
            return null;
        }
    }
}