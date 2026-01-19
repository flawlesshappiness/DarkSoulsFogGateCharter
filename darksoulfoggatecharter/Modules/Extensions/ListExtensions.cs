using Godot;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
    public static T Random<T>(this List<T> list, RandomNumberGenerator rng = null)
    {
        rng ??= new RandomNumberGenerator();
        return list.Count > 0 ? list.GetOrDefault(rng.RandiRange(0, list.Count - 1)) : default(T);
    }

    public static T PopRandom<T>(this List<T> list, RandomNumberGenerator rng = null)
    {
        var v = list.Random(rng);
        list.Remove(v);
        return v;
    }

    public static T GetClamped<T>(this List<T> list, int index)
    {
        if (list.Count == 0) return default(T);
        return list[Mathf.Clamp(index, 0, list.Count - 1)];
    }

    public static T GetOrDefault<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count) return default(T);
        return list[index];
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        list = list.ToList();
        var rng = new RandomNumberGenerator();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.RandiRange(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
}
