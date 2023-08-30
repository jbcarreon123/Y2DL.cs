namespace Y2DL.Utils;

public static class IntUtils
{
    public static string ToFormattedNumber(this ulong num)
    {
        // Define the magnitude limits for K (thousands) and M (millions).
        double thousand = 1000.0;
        double million = 1000000.0;

        double number = num;

        // Check if the number is in thousands or millions range and format accordingly.
        if (Math.Abs(number) >= million)
        {
            return (number / million).ToString("0.0") + "M";
        }
        else if (Math.Abs(number) >= thousand)
        {
            return (number / thousand).ToString("0.0") + "K";
        }
        else
        {
            return number.ToString();
        }
    }
}