using Godot;

public static class ResourceExtensions
{
    public static string GetResourceName(this Resource r)
    {
        return System.IO.Path.GetFileNameWithoutExtension(r.ResourcePath);
    }
}
