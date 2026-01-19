using System;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> predicate)
    {
        foreach (var item in source)
        {
            predicate(item);
        }

        return source;
    }

    public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count, bool allow_duplicates = false)
    {
        var result = new List<T>();
        var list = source.ToList();

        for (int i = 0; i < count; i++)
        {
            var v = list.Random();
            if (v == null) break;
            if (!allow_duplicates) list.Remove(v);
            result.Add(v);
        }

        return result;
    }
}
