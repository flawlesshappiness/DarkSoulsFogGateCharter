using Godot;
using System;
using System.Text.Json;

public partial class SaveDataController : Node
{
    public static SaveDataController Instance => Singleton.GetOrCreate<SaveDataController>($"{Paths.Modules}/Data/{nameof(SaveDataController)}");

    public T GetOrCreate<T>(int? profile = null)
        where T : SaveData, new()
    {
        Debug.TraceMethod($"{typeof(T)} {profile}");

        try
        {
            var path = GetSaveDataFilePath<T>(profile);
            if (FileAccess.FileExists(path))
            {
                return DeserializeFileFromPath<T>(path);
            }
            else
            {
                var data = Create<T>(profile);
                return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message}");
            return new T();
        }
    }

    public T Create<T>(int? profile = null)
        where T : SaveData, new()
    {
        var path = GetSaveDataFilePath<T>(profile);
        EnsureFileExists(path);

        var data = new T();
        data.Profile = profile;
        data.Deleted = true;
        Save(data);

        return data;
    }

    public void Save(SaveData data)
    {
        var type = data.GetType();

        Debug.TraceMethod(type.Name);
        Debug.Indent++;

        data.Update();

        var json = JsonSerializer.Serialize(data, data.GetType(), new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
        var filename = type.Name;
        var path = GetSaveDataFilePath(type, data.Profile);
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreLine(json);

        Debug.Indent--;
    }

    private void EnsureFileExists(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            using (FileAccess.Open(path, FileAccess.ModeFlags.Write)) { }
            Debug.Log($"Created file at path: {path}");
        }
    }

    private T EnsureBetaFileIsNewestVersion<T>(T data)
        where T : SaveData, new()
    {
        var current_version = Version.Parse(ApplicationInfo.Instance.Version);
        var data_version = Version.Parse(data.Version);
        var is_lesser_version = data_version.CompareTo(current_version) < 0;

        if (!data.IsRelease && is_lesser_version)
        {
            Debug.Log($"Data version {data_version} < current version {current_version} - Creating new save");
            data = Create<T>();
        }

        return data;
    }

    private string GetSaveDataFilePath<T>(int? profile = null)
        where T : SaveData
    {
        var type = typeof(T);
        return GetSaveDataFilePath(type, profile);
    }

    private string GetSaveDataFilePath(Type type, int? profile = null)
    {
        var filename = type.Name;
        var profile_string = profile?.ToString() ?? string.Empty;
        var path = $"user://{filename}{profile_string}.save";
        return path;
    }

    private T DeserializeFileFromPath<T>(string path)
        where T : SaveData, new()
    {
        var json = FileAccess.GetFileAsString(path);
        return JsonSerializer.Deserialize<T>(json);
    }
}
