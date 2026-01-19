using Godot;
using System;

public partial class GameTime : Node
{
    public static float DeltaTime { get; private set; }
    public static float Time { get; private set; }
    public static float UnscaledTime { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        DeltaTime = Convert.ToSingle(delta);
        Time += Scene.PauseLock.IsFree ? DeltaTime : 0;
        UnscaledTime += DeltaTime;
    }

    public static float T(float start, float duration)
    {
        return (Time - start) / duration;
    }

    public static DateTime ParseDateTime(string date_string)
    {
        return DateTime.ParseExact(date_string, "yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string GetDateTimeString(DateTime date)
    {
        return date.ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static DateTime GetCurrentDateTime()
    {
        return ParseDateTime(Godot.Time.GetDatetimeStringFromSystem(true));
    }

    public static string GetCurrentDateTimeString()
    {
        return GetDateTimeString(GetCurrentDateTime());
    }
}
