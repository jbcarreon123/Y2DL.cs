namespace Y2DL.Utils;

public static class UlongExtensions
{
    public static ulong ToUlong(this ulong? @ulong)
    {
        return (ulong)@ulong;
    }

    public static ulong ToUlong(this long @long)
    {
        return (ulong)@long;
    }
}