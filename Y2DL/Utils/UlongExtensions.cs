namespace Y2DL.Utils;

public static class UlongExtensions
{
    public static string ToFormattedNumber(this ulong? num)
    {
        // Define the magnitude limits for K (thousands) and M (millions).
        var thousand = 1000.0;
        var million = 1000000.0;

        double number = num ?? 0;

        // Check if the number is in thousands or millions range and format accordingly.
        if (Math.Abs(number) >= million)
            return (number / million).ToString("0.0") + "M";
        if (Math.Abs(number) >= thousand)
            return (number / thousand).ToString("0.0") + "K";
        return number.ToString();
    }

    public static ulong? ToUlong(this ulong? @ulong)
    {
        return (ulong?)@ulong;
    }

    public static ulong? ToUlong(this long? @long)
    {
        return (ulong?)@long;
    }
    
    public static ulong ToUlong(this long @long)
    {
        return (ulong)@long;
    }
}