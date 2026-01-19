using Godot;

public static class MathHelper
{
    public static float Percentage(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static double Percentage(double value, double min, double max)
    {
        return (value - min) / (max - min);
    }

    public static float Percentage(int value, int min, int max)
    {
        return Percentage((float)value, min, max);
    }

    public static Vector2 CirclePoint(float radius, float angleInDegrees)
    {
        var rad = Mathf.DegToRad(angleInDegrees);
        var x = radius * Mathf.Cos(rad);
        var y = radius * Mathf.Sin(rad);
        return new Vector2(x, y);
    }
}
