using Godot;

public static class AnimationTreeExtensions
{
    public static bool GetParameter(this AnimationTree tree, string name)
    {
        return tree.Get($"parameters/conditions/{name}").AsBool();
    }

    public static void SetParameter(this AnimationTree tree, string name, bool value)
    {
        tree.Set($"parameters/conditions/{name}", value);
    }

    public static void ToggleParameter(this AnimationTree tree, string name)
    {
        tree.Set($"parameters/conditions/{name}", !tree.GetParameter(name));
    }
}
