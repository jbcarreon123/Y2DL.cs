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

    public static BotCustomStatus Random(this BotCustomStatus[] customstatuses)
    {
        Random random = new Random();
        var rnd = random.Next(0, customstatuses.Length - 1);

        return customstatuses[rnd];
    }

    public static BotCustomStatus Random(this List<BotCustomStatus>? customstatuses)
        => Random(customstatuses.ToArray());
}