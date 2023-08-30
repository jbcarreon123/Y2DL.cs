using Y2DL.Models;

namespace Y2DL.Utils;

public static class ArrayUtils
{
    public static ApiKeys LoopAbout(this List<ApiKeys> obj, int value)
    {
        if (obj.Count == value + 1)
        {
            return obj[0];
        }
        else
        {
            return obj[value + 1];
        }
    }
}