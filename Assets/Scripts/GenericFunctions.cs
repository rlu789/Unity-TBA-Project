using System;
using System.Collections.Generic;

public static class GenericFunctions {

    public static void Shuffle<T>(this IList<T> list, Random rnd)
    {
        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rnd.Next(i, list.Count));
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

}
