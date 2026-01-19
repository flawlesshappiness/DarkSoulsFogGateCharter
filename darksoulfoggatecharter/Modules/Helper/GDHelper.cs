using Godot;
using System.Text;

public partial class GDHelper
{
    public static T Instantiate<T>(string scene_path) where T : GodotObject
    {
        var packed_scene = LoadPackedScene(scene_path);
        var instance = packed_scene.Instantiate<T>();
        return instance;
    }

    private static PackedScene LoadPackedScene(string scene_path)
    {
        var prefix = "res://";
        var ext = ".tscn";
        var sb = new StringBuilder();
        if (!scene_path.StartsWith(prefix)) sb.Append(prefix);
        sb.Append(scene_path);
        if (!scene_path.EndsWith(ext)) sb.Append(ext);
        var path = sb.ToString();

        var packed_scene = (PackedScene)GD.Load(path);
        return packed_scene;
    }
}
