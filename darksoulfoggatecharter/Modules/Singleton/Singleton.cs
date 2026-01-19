using Godot;
using System;
using System.Collections.Generic;

public static class Singleton
{
    private static Dictionary<string, Node> _singletons = new();

    private static T CreateInstance<T>(string script_path) where T : Node
    {
        var node = CreateInstance(script_path, typeof(T));

        if (node.TryGetNode<T>(out var instance))
        {
            Debug.Log($"Created instance: {typeof(T).Name}");
            return instance;
        }

        throw new System.NullReferenceException("Failed to get script instance on node");
    }

    private static Node CreateInstance(string script_path, Type type)
    {
        var node = new Node();
        node.Name = type.Name;

        script_path = $"res://{script_path}.cs";
        var script = GD.Load<Script>(script_path);
        var node_id = node.GetInstanceId();
        node.SetScript(script);
        node = Node.InstanceFromId(node_id) as Node;

        Scene.Root.AddChild(node);

        return node;
    }

    public static T GetOrCreate<T>(string script_path) where T : Node => GetOrCreate(script_path, typeof(T)) as T;
    public static Node GetOrCreate(string script_path, Type type)
    {
        if (!TryGet(type, out var singleton))
        {
            Debug.TraceMethod($"{script_path}, {type.Name}");
            Debug.Indent++;

            singleton = CreateInstance(script_path, type);
            _singletons.Add(type.Name, singleton);

            Debug.Trace($"Created singleton: {type.Name}");
            Debug.Indent--;
        }

        return singleton;
    }

    public static T LoadScene<T>(string scene_path) where T : Node => LoadScene(scene_path, typeof(T)) as T;
    public static Node LoadScene(string scene_path, Type type)
    {
        if (!TryGet(type, out var singleton))
        {
            singleton = GDHelper.Instantiate<Node>(scene_path);
            singleton.SetParent(Scene.Root);
            _singletons.Add(type.Name, singleton);
        }

        return singleton;
    }

    public static T Get<T>() where T : Node => Get(typeof(T)) as T;
    public static Node Get(Type type)
    {
        var name = type.Name;
        return _singletons.ContainsKey(name) ? _singletons[name] : throw new Exception($"Failed to get singleton with type: {type}");
    }

    public static bool TryGet<T>(out T result) where T : Node
    {
        var success = TryGet(typeof(T), out var node_result);
        result = node_result as T;
        return success;
    }
    public static bool TryGet(Type type, out Node result)
    {
        try
        {
            result = Get(type);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}
