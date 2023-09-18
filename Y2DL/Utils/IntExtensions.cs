namespace Y2DL.Utils;

public static class IntExtensions
{
    public static double Round(this double input, int dec = 0)
    {
        return Math.Round(input, dec);
    }
}