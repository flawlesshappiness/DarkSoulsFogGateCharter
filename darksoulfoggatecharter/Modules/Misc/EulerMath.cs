using Godot;
using System.Collections.Generic;
using System.Linq;

public static class EulerMath
{
    public static Vector3 WrappedEulerAngles(Vector3 v)
    {
        return new Vector3(
            WrappedEulerAngle(v.X),
            WrappedEulerAngle(v.Y),
            WrappedEulerAngle(v.Z));
    }

    public static float WrappedEulerAngle(float v)
    {
        v %= 360;
        v += v < -180 ? 360 : v > 180 ? -360 : 0;
        return v;
    }

    public static Vector3 ClosestEulerAngles(Vector3 from, Vector3 to)
    {
        return new Vector3(
            ClosestEulerAngle(from.X, to.X),
            ClosestEulerAngle(from.Y, to.Y),
            ClosestEulerAngle(from.Z, to.Z)
            );
    }

    public static float ClosestEulerAngle(float from, float to)
    {
        var list = new List<float> { from, from - 360, from + 360 };
        var closest = list
            .OrderBy(x => Mathf.Abs(x - to))
            .First();
        return closest;
    }
}
