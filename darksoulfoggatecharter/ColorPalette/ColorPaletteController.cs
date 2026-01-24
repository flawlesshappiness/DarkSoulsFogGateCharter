using Godot;
using System.Linq;

public partial class ColorPaletteController : ResourceController<ColorPaletteCollection, ColorPaletteInfo>
{
    public static ColorPaletteController Instance => Singleton.Get<ColorPaletteController>();
    public override string Directory => "ColorPalette";

    public ColorPaletteInfo GetInfo(string name)
    {
        var info = Collection.Resources.FirstOrDefault(x => x.Name == name);

        if (info == null)
        {
            Debug.LogError($"Failed to get ColorPaletteInfo for {name}");
            info = Collection.Resources.First();
        }

        return info;
    }

    public Color GetColor(string name, int index)
    {
        return GetInfo(name)?.GetColor(index) ?? Collection.Resources.First().GetColor(index); ;
    }
}
