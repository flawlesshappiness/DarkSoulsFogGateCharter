using Godot;
using System.Collections.Generic;
using System.IO;

[GlobalClass]
public partial class SoundCollection : ResourceCollection<SoundInfo>
{
    private Dictionary<string, SoundEntry> _entries = new();

    protected override void OnLoad()
    {
        base.OnLoad();
        MapEntries();
    }

    private void MapEntries()
    {
        foreach (var info in Resources)
        {
            var filename = Path.GetFileName(info.ResourcePath).RemoveExtension();
            _entries.Add(filename, new SoundEntry
            {
                Info = info,
            });
        }
    }

    public SoundEntry GetEntry(string filename)
    {
        filename = GetFilename(filename);

        if (_entries.TryGetValue(filename, out var entry))
        {
            return entry;
        }

        Debug.LogError("Failed to get sound entry with name: " + filename);
        return null;
    }
}

public class SoundEntry
{
    public SoundInfo Info { get; set; }
    public AudioStream LastPlayedStream { get; set; }
}