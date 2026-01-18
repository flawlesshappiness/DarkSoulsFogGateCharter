using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GateController : Node
{
    public static GateController Instance { get; private set; }

    [Export(PropertyHint.File)]
    public string GatesPath;

    public MainScene Scene => MainScene.Instance;
    public Dictionary<string, GateNode> Gates { get; private set; } = new();
    public Dictionary<string, GateGroup> Groups { get; private set; } = new();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        ParseGatesFile();
    }

    public void ClearData()
    {
        foreach (var gate in Gates.Values)
        {
            gate.Data = new();
        }
    }

    private void ParseGatesFile()
    {
        var file = FileAccess.GetFileAsString(GatesPath);
        var lines = file.Split("\n");
        var groups = new Dictionary<string, GateGroup>();

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split(',');
            var id = data[0];
            var name = data[1];
            var type = data[2];
            var connection = data[3];

            var gate = new GateNode
            {
                Id = id,
                Name = name,
                Connection = connection,
                Type = type,
            };

            Gates.Add(name, gate);

            // Create possible groups
            if (!groups.ContainsKey(connection))
            {
                groups.Add(connection, new GateGroup { Name = connection });
            }
            groups[connection].Gates.Add(name, gate);
        }

        // Add valid groups
        foreach (var group in groups.Values.Where(x => x.Gates.Count > 1))
        {
            Groups.Add(group.Name, group);
        }
    }

    public GateNode GetGate(string name) =>
        Gates.TryGetValue(name, out var gate) ? gate : null;

    public GateGroup GetGroup(string name) =>
        Groups.TryGetValue(name, out var group) ? group : null;

    public bool IsGroup(string name) =>
        Groups.ContainsKey(name);

    public bool IsGateInGroup(string name) =>
        Groups.Values.Any(x => x.Gates.ContainsKey(name));

    public bool ShouldAutoGenerate(string name)
    {
        if (IsGroup(name))
        {
            return true;
        }
        else
        {
            var gate = GetGate(name);
            if (gate == null) return false;

            var item = gate.Type == GateType.ItemObtained;
            var door = gate.Type == GateType.DoorShortcut;
            var boss_killed = gate.Type == GateType.BossKilled;
            var always = item || door || boss_killed;
            return always;
        }
    }

    public bool IsOneWay(string name)
    {
        if (IsGroup(name))
        {
            return false;
        }
        else
        {
            var gate = GetGate(name);
            if (gate == null) return false;

            return gate.Type == GateType.OnewayShortcut;
        }
    }

    public bool IsSearchable(string name)
    {
        if (IsGroup(name))
        {
            return false;
        }
        else
        {
            var missing_connections = !Scene.IsNodeFullyConnected(name);
            var gate = GetGate(name);
            var no_id = string.IsNullOrEmpty(gate.Id);
            var item = gate.Type == GateType.ItemObtained;
            var door = gate.Type == GateType.DoorShortcut;
            var oneway = gate.Type == GateType.OnewayShortcut;
            var boss_killed = gate.Type == GateType.BossKilled;
            var never = !(no_id || item || door || oneway || boss_killed);
            return never && missing_connections;
        }
    }
}
