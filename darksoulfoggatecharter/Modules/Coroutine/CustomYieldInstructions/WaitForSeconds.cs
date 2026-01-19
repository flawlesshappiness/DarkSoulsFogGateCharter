public class WaitForSeconds : CustomYieldInstruction
{
    public double StartTime { get; private set; }
    public double EndTime { get; private set; }
    public double CurrentTime => UnscaledTime ? GameTime.UnscaledTime : GameTime.Time;
    public override bool KeepWaiting => CurrentTime < EndTime;
    public bool UnscaledTime { get; protected set; }

    public WaitForSeconds(double seconds, bool unscaled = false)
    {
        UnscaledTime = unscaled;
        StartTime = CurrentTime;
        EndTime = StartTime + seconds;
    }

    public override string ToString()
    {
        return base.ToString() + $"({CurrentTime - StartTime} / {EndTime - StartTime})";
    }
}

public class WaitForSecondsUnscaled : WaitForSeconds
{
    public WaitForSecondsUnscaled(double seconds) : base(seconds, true)
    {
    }
}