using System;
using System.Collections.Generic;

namespace Y2DL.Utils;

public static class ListExtensions
{
    public static T Next<T>(this List<T> list, T currentElement)
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        int currentIndex = list.IndexOf(currentElement);

        if (currentIndex == -1 || currentIndex == list.Count - 1)
        {
            return list[0];
        }
        else
        {
            return list[currentIndex + 1];
        }
    }
    
    public static void MoveFirstToLast<T>(this List<T> list)
    {
        if (list.Count > 1)
        {
            T firstItem = list[0];
            list.RemoveAt(0);
            list.Add(firstItem);
        }
    }
}
