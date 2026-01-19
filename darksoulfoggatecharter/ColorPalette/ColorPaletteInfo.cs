using Godot;
using Godot.Collections;
using System.Linq;

[GlobalClass]
public partial class ColorPaletteInfo : Resource
{
    [Export]
    public string Name;

    [Export]
    public Array<Color> Colors;

    public Color GetColor(int i)
    {
        return Colors.ToList().GetClamped(i);
    }
}
