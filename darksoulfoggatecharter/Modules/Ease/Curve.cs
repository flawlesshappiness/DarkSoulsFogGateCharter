public class Curve
{
    private EasingFunctions.Function _function;

    public EasingFunctions.Function EaseFunction => _function ?? (_function = EasingFunctions.GetEasingFunction(Ease));
    public EasingFunctions.Ease Ease { get; set; }

    public Curve(EasingFunctions.Ease ease)
    {
        Ease = ease;
    }

    public float Evaluate(float t)
    {
        var start = 0;
        var end = 1;
        return EaseFunction(start, end, t);
    }
}
