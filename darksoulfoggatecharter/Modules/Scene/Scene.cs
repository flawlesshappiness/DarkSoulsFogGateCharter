using Godot;

public partial class Scene : NodeScript
{
    private bool _initialized;

    public bool IsPaused => GetTree().Paused;

    public static Scene Current { get; set; }
    public static SceneTree Tree { get; set; }
    public static Window Root { get; set; }
    public static MultiLock PauseLock { get; } = new();

    protected virtual void OnDestroy() { }

    public static T Instantiate<T>(string path) where T : Scene
    {
        var scene = GDHelper.Instantiate<T>(path);
        scene.SetParent(Scene.Root);
        return scene;
    }

    public static Scene Goto(string scene_name)
    {
        Debug.TraceMethod(scene_name);
        Debug.Indent++;

        if (string.IsNullOrEmpty(scene_name))
        {
            Debug.LogError("Scene name was empty");
            Debug.Indent--;
            return Current;
        }

        if (Current != null)
        {
            Current.Destroy();
        }

        Current = Instantiate<Scene>($"Scenes/{scene_name}");
        Debug.TraceMethod($"Current: {Current}");

        Debug.Indent--;
        return Current;
    }

    public static T Goto<T>() where T : Scene =>
        Goto(typeof(T).Name) as T;

    public void Destroy() => Destroy(this);

    public static void Destroy(Scene scene)
    {
        scene.OnDestroy();
        scene.QueueFree();
    }
}
