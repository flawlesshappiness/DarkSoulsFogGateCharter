using Godot;
using System;
using System.Collections;

public class LerpEnumerator : CustomYieldInstruction
{
    public override bool KeepWaiting => Enumerator.MoveNext();

    public float Duration { get; private set; }

    public IEnumerator Enumerator { get; private set; }

    public Curve Curve { get; private set; }

    public Action<float> LerpAction { get; private set; }

    public LerpEnumerator(float duration, Action<float> lerpAction)
    {
        Duration = duration;
        LerpAction = lerpAction;
        Enumerator = LerpEnumerator.Lerp01(duration, DoLerpAction);
    }

    private void DoLerpAction(float t)
    {
        var tc = Curve == null ? t : Curve.Evaluate(t);
        LerpAction(tc);
    }

    public LerpEnumerator SetCurve(Curve curve)
    {
        Curve = curve;
        return this;
    }

    public LerpEnumerator SetCurve(EasingFunctions.Ease ease)
    {
        var curve = new Curve(ease);
        return SetCurve(curve);
    }

    public static IEnumerator Lerp01(float duration, Action<float> onLerp)
    {
        return _Lerp01(duration, false, onLerp);
    }

    public static IEnumerator Lerp01_Unscaled(float duration, Action<float> onLerp)
    {
        return _Lerp01(duration, true, onLerp);
    }

    private static IEnumerator _Lerp01(float duration, bool unscaled, Action<float> onLerp)
    {
        var start = GetTime();
        while (true)
        {
            var v = duration == 0 ? 1 : (GetTime() - start) / duration;
            var t = Math.Clamp(v, 0, 1);
            var tf = Convert.ToSingle(t);
            onLerp?.Invoke(tf);
            yield return null;
            if (t >= 1)
            {
                onLerp?.Invoke(1);
                break;
            }
        }

        float GetTime() => unscaled ? GameTime.UnscaledTime : GameTime.Time;
    }

    #region PROPERTY
    public static LerpEnumerator Property(Node node, string property, float duration, Variant end) => Property(node, property, duration, node.Get(property), end);
    public static LerpEnumerator Property(Node node, string property, float duration, Variant start, Variant end) =>
        new LerpEnumerator(duration, t => node.Set(property, Lerp.Variant(start, end, t)));
    #endregion
    #region POSITION
    public static LerpEnumerator Position(Node node, float duration, Vector2 end) => Property(node, "position", duration, end);
    public static LerpEnumerator Position(Node node, float duration, Vector2 start, Vector2 end) => Property(node, "position", duration, start, end);
    #endregion
    #region GLOBAL POSITION
    public static LerpEnumerator GlobalPosition(Control node, float duration, Vector2 end) => Property(node, "global_position", duration, end);
    public static LerpEnumerator GlobalPosition(Control node, float duration, Vector2 start, Vector2 end) => Property(node, "global_position", duration, start, end);
    #endregion
}
