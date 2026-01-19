using Godot;

public static class ColorExtensions
{
    public static Color Set(this Color c, float? r = null, float? g = null, float? b = null, float? a = null)
    {
        return new Color(r ?? c.R, g ?? c.G, b ?? c.B, a ?? c.A);
    }

    public static Color SetR(this Color c, float r) => c.Set(r: r);
    public static Color SetG(this Color c, float g) => c.Set(g: g);
    public static Color SetB(this Color c, float b) => c.Set(b: b);
    public static Color SetA(this Color c, float a) => c.Set(a: a);
}
