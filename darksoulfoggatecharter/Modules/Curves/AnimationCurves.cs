using Godot;

[GlobalClass]
public partial class AnimationCurves : Resource
{
    public static AnimationCurves Instance => _instance ?? (_instance = Load());
    private static AnimationCurves _instance;

    private static AnimationCurves Load()
    {
        var path_file = $"res://Modules/Curves/{nameof(AnimationCurves)}.tres";
        var resource = GD.Load<AnimationCurves>(path_file);
        return resource;
    }

    public static Godot.Curve WobbleOut(float max, int peaks)
    {
        var curve = new Godot.Curve();
        curve.AddPoint(new Vector2(0, 1), rightTangent: 2);

        var x_curve = Curves.EaseInSine;
        for (int i = 0; i < peaks; i++)
        {
            var sign = i % 2 == 0 ? 1 : -1;
            var t_peak = (float)i / peaks;
            var y = 1f + Mathf.Lerp(max, 0, t_peak) * sign;

            var t_x = (float)(i + 1) / (peaks + 1);
            var t_x_curve = x_curve.Evaluate(t_x);
            var x = Mathf.Lerp(0f, 1f, t_x_curve);

            var point = new Vector2(x, y);

            curve.AddPoint(point);
        }

        curve.AddPoint(new Vector2(1, 1));
        return curve;
    }
}
