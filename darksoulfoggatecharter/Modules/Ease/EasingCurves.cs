public static class Curves
{
    public static Curve EaseInQuad { get { return GetCurve(EasingFunctions.Ease.EaseInQuad); } }
    public static Curve EaseOutQuad { get { return GetCurve(EasingFunctions.Ease.EaseOutQuad); } }
    public static Curve EaseInOutQuad { get { return GetCurve(EasingFunctions.Ease.EaseInOutQuad); } }
    public static Curve EaseInCubic { get { return GetCurve(EasingFunctions.Ease.EaseInCubic); } }
    public static Curve EaseOutCubic { get { return GetCurve(EasingFunctions.Ease.EaseOutCubic); } }
    public static Curve EaseInOutCubic { get { return GetCurve(EasingFunctions.Ease.EaseInOutCubic); } }
    public static Curve EaseInQuart { get { return GetCurve(EasingFunctions.Ease.EaseInQuart); } }
    public static Curve EaseOutQuart { get { return GetCurve(EasingFunctions.Ease.EaseOutQuart); } }
    public static Curve EaseInOutQuart { get { return GetCurve(EasingFunctions.Ease.EaseInOutQuart); } }
    public static Curve EaseInQuint { get { return GetCurve(EasingFunctions.Ease.EaseInQuint); } }
    public static Curve EaseOutQuint { get { return GetCurve(EasingFunctions.Ease.EaseOutQuint); } }
    public static Curve EaseInOutQuint { get { return GetCurve(EasingFunctions.Ease.EaseInOutQuint); } }
    public static Curve EaseInSine { get { return GetCurve(EasingFunctions.Ease.EaseInSine); } }
    public static Curve EaseOutSine { get { return GetCurve(EasingFunctions.Ease.EaseOutSine); } }
    public static Curve EaseInOutSine { get { return GetCurve(EasingFunctions.Ease.EaseInOutSine); } }
    public static Curve EaseInExpo { get { return GetCurve(EasingFunctions.Ease.EaseInExpo); } }
    public static Curve EaseOutExpo { get { return GetCurve(EasingFunctions.Ease.EaseOutExpo); } }
    public static Curve EaseInOutExpo { get { return GetCurve(EasingFunctions.Ease.EaseInOutExpo); } }
    public static Curve EaseInCirc { get { return GetCurve(EasingFunctions.Ease.EaseInCirc); } }
    public static Curve EaseOutCirc { get { return GetCurve(EasingFunctions.Ease.EaseOutCirc); } }
    public static Curve EaseInOutCirc { get { return GetCurve(EasingFunctions.Ease.EaseInOutCirc); } }
    public static Curve Linear { get { return GetCurve(EasingFunctions.Ease.Linear); } }
    public static Curve Spring { get { return GetCurve(EasingFunctions.Ease.Spring); } }
    public static Curve EaseInBounce { get { return GetCurve(EasingFunctions.Ease.EaseInBounce); } }
    public static Curve EaseOutBounce { get { return GetCurve(EasingFunctions.Ease.EaseOutBounce); } }
    public static Curve EaseInOutBounce { get { return GetCurve(EasingFunctions.Ease.EaseInOutBounce); } }
    public static Curve EaseInBack { get { return GetCurve(EasingFunctions.Ease.EaseInBack); } }
    public static Curve EaseOutBack { get { return GetCurve(EasingFunctions.Ease.EaseOutBack); } }
    public static Curve EaseInOutBack { get { return GetCurve(EasingFunctions.Ease.EaseInOutBack); } }
    public static Curve EaseInElastic { get { return GetCurve(EasingFunctions.Ease.EaseInElastic); } }
    public static Curve EaseOutElastic { get { return GetCurve(EasingFunctions.Ease.EaseOutElastic); } }
    public static Curve EaseInOutElastic { get { return GetCurve(EasingFunctions.Ease.EaseInOutElastic); } }

    public static Curve GetCurve(EasingFunctions.Ease ease)
    {
        return new Curve(ease);
    }
}
