using Godot;
using System;

public static class AudioMath
{
    public static float PercentageToDecibel(float value)
    {
        var t = Math.Max(value, 0.0001f);
        var dcb = Math.Log10(t) * 20;
        return Convert.ToSingle(dcb);
    }

    public static float DecibelToPercentage(float value)
    {
        return Mathf.Pow(10, value / 20f);
    }

    public static float LerpPercentageToDecibel(float start_perc, float end_perc, float t)
    {
        return PercentageToDecibel(Mathf.Lerp(start_perc, end_perc, t));
    }

    public static float LerpDecibelToPercentage(float start_db, float end_db, float t)
    {
        return DecibelToPercentage(Mathf.Lerp(start_db, end_db, t));
    }
}
