using System.Xml;

namespace Y2DL.Utils;

public static class TimeSpanUtils
{
    public static string ToFormattedString(this TimeSpan timeSpan)
    {
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;

        // Use a StringBuilder for efficient string concatenation
        var formattedDuration = new System.Text.StringBuilder();

        // Add hours if they are greater than 0
        if (hours > 0)
        {
            formattedDuration.Append(hours);
            formattedDuration.Append(':');
        }

        // Always add minutes and seconds
        formattedDuration.AppendFormat("{0:D2}:{1:D2}", minutes, seconds);

        return formattedDuration.ToString();
    }

    public static TimeSpan ToTimeSpan(this string str)
    {
        try
        {
            return XmlConvert.ToTimeSpan(str);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}