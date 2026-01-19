using Godot;
using System.Collections.Generic;
using System.IO;

public partial class ResourceCollection<T> : Resource
    where T : Resource
{
    private List<T> _resources;
    public List<T> Resources => _resources;

    protected Dictionary<string, T> _resource_maps = new();

    public static C Load<C>(string path) where C : ResourceCollection<T>
    {
        Debug.TraceMethod(path);
        Debug.Indent++;

        var collection = GD.Load<C>(path);
        var filename_collection = Path.GetFileName(path);
        var path_dir = "res://" + path.Replace(filename_collection, "");
        var dir = DirAccess.Open(path_dir);
        var files = dir.GetFiles();

        var resources = new List<T>();
        foreach (var file in files)
        {
            try
            {
                var filename = file.Replace(".remap", "");
                var path_file = $"{path_dir}{filename}";
                var resource = GD.Load<T>(path_file);
                Debug.Trace("Resource loaded: " + path_file);
                resources.Add(resource);
            }
            catch
            {
                continue;
            }
        }

        collection.SetResources(resources);
        collection.OnLoad();
        Debug.Indent--;
        return collection;
    }

    private void SetResources(List<T> resources) => _resources = resources;

    protected virtual void OnLoad()
    {
        MapResources();
    }

    private void MapResources()
    {
        foreach (var info in Resources)
        {
            var filename = Path.GetFileName(info.ResourcePath).RemoveExtension();
            _resource_maps.Add(filename, info);
        }
    }

    protected Dictionary<string, X> LoadResources<X>(string path)
        where X : class
    {
        var dir = DirAccess.Open(path);
        var files = dir.GetFiles();
        var results = new Dictionary<string, X>();

        foreach (var file in files)
        {
            try
            {
                var path_file = $"{path}/{file}";
                var ext = path_file.GetExtension();
                if (ext != "import") continue;

                var resource = GD.Load<X>(path_file.RemoveExtension());
                var filename = GetFilename(path_file);
                results.Add(filename, resource);
            }
            catch
            {
                continue;
            }
        }

        return results;
    }

    protected string GetFilename(string path)
    {
        return Path.GetFileName(path)?.RemoveExtensions();
    }

    public T GetResource(string name)
    {
        return _resource_maps.TryGetValue(name, out var resource) ? resource : null;
    }
}
