using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GateController : SingletonController
{
    public static GateController Instance { get; private set; }

    public NodeController Node => NodeController.Instance;
    public Dictionary<string, GateNode> Gates { get; private set; } = new();
    public Dictionary<string, GateGroup> Groups { get; private set; } = new();
    public List<string> DisabledTypes { get; private set; } = new();

    public override string Directory => "Gate";

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        ParseGatesFile();
    }

    public void LoadSettings(SessionData data)
    {
        DisabledTypes = data.DisabledTypes.ToList();
    }

    private void ParseGatesFile()
    {
        var file = FileAccess.GetFileAsString(GateInfo.Instance.GatesPath);
        var lines = file.Split("\n");
        var groups = new Dictionary<string, GateGroup>();

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split(',');
            var id = data[0];
            var name = data[1];
            var type = data[2];
            var location = data[3];
            var area = data[4];

            var gate = new GateNode
            {
                Id = id,
                Name = name,
                Location = location,
                Type = type,
                Area = area,
            };

            Gates.Add(name, gate);

            // Create possible groups
            if (!string.IsNullOrEmpty(location))
            {
                if (!groups.ContainsKey(location))
                {
                    groups.Add(location, new GateGroup { Name = location, Area = area });
                }
                groups[location].Gates.Add(name, gate);
            }
        }

        // Add valid groups
        foreach (var group in groups.Values.Where(x => x.Gates.Count > 2))
        {
            Groups.Add(group.Name, group);
        }
    }

    public GateNode GetGate(string name) =>
        Gates.TryGetValue(name, out var gate) ? gate : null;

    public IEnumerable<GateNode> GetGatesById(string id) =>
        Gates.Values.Where(x => x.Id == id);

    public IEnumerable<GateNode> GetGatesByLocation(string location) =>
        Gates.Values.Where(x => x.Location == location);

    public GateGroup GetGroup(string name) =>
        Groups.TryGetValue(name, out var group) ? group : null;

    public bool IsGroup(string name) =>
        Groups.ContainsKey(name);

    public bool IsGateInGroup(string name) =>
        Groups.Values.Any(x => x.Gates.ContainsKey(name));

    public GateNode GetGateExit(string name)
    {
        var gate = GetGate(name);
        var exits = GetGatesById(gate.Id);
        var exit = exits.FirstOrDefault(x => x != gate);
        return exit;
    }

    public bool ShouldAutoGenerate(string name)
    {
        if (IsGroup(name))
        {
            return true;
        }
        else
        {
            var gate = GetGate(name);
            return true;
        }
    }

    public bool IsDisabled(string name)
    {
        if (IsGroup(name))
        {
            return false;
        }
        else
        {
            var gate = GetGate(name);
            return DisabledTypes.Contains(gate.Type);
        }
    }

    public bool IsShortcut(string name)
    {
        if (IsGroup(name))
        {
            return false;
        }
        else
        {
            var gate = GetGate(name);
            var door = gate.Type == GateType.DoorShortcut;
            var oneway = gate.Type == GateType.OnewayShortcut;
            return door || oneway;
        }
    }

    public bool IsExit(string name)
    {
        if (IsGroup(name))
        {
            return false;
        }
        else
        {
            var gate = GetGate(name);
            var exit = gate.Type == GateType.ShortcutExit;
            return exit;
        }
    }

    public bool ShowInGroup(string name)
    {
        if (IsGroup(name))
        {
            return true;
        }
        else
        {
            var gate = GetGate(name);
            var shortcut_exit = gate.Type == GateType.ShortcutExit;
            var never = !(shortcut_exit);
            return never;
        }
    }

    public bool IsSearchable(string name, bool from_new = false)
    {
        if (IsGroup(name))
        {
            return false;
        }
        else
        {
            var missing_connections = from_new || !Node.IsNodeFullyConnected(name);
            var disabled = from_new ? false : IsDisabled(name);
            var gate = GetGate(name);
            var no_id = string.IsNullOrEmpty(gate.Id);
            var objective = gate.Type == GateType.Objective;
            var door = gate.Type == GateType.DoorShortcut;
            var boss = gate.Type == GateType.Boss;
            var shortcut_door = gate.Type == GateType.DoorShortcut;
            var shortcut_oneway = gate.Type == GateType.OnewayShortcut;
            var shortcut_exit = gate.Type == GateType.ShortcutExit;
            var never = !(no_id || objective || door || shortcut_door || shortcut_oneway || shortcut_exit || boss || disabled);
            return never && missing_connections;
        }
    }
}
