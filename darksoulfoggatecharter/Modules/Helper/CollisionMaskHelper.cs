using System;
using System.Linq;

public static class CollisionMaskHelper
{
    public static uint Create(params Enum[] enums)
    {
        var layers = enums.Select(x => Convert.ToInt32(x)).ToArray();
        return Create(layers);
    }

    public static uint Create(params int[] layers)
    {
        uint value = default(uint);

        foreach (var layer in layers)
        {
            value |= (uint)(1 << (layer - 1));
        }

        return value;
    }
}

public enum CollisionMaskType
{
    World = 1,
    Item = 2,
    Interact = 3,
    Player = 5,
}
