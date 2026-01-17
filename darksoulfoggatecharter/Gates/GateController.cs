using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class GateController : Node
{
    public static GateController Instance { get; private set; }

    [Export(PropertyHint.File)]
    public string GatesPath;

    public List<GateNode> Gates { get; private set; } = new();
    public List<AreaNode> Areas { get; private set; } = new();
    public List<GateConnection> Connections { get; private set; } = new();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        ParseGatesFile();
        CallDeferred("Initialize");
    }

    public void Initialize()
    {

    }

    private void ParseGatesFile()
    {
        var file = FileAccess.GetFileAsString(GatesPath);
        var lines = file.Split("\n");

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split(',');
            var id = data[0];
            var gate = data[1];
            var area = data[2];
            var type = data[3];

            var node = new GateNode
            {
                Id = id,
                Name = gate,
                Area = area,
                Type = type,
            };

            AddGate(node);
        }
    }

    private void AddGate(GateNode node)
    {
        Gates.Add(node);

        var area = GetOrCreateArea(node.Area);
        area.Gates.Add(node);
    }

    public AreaNode GetOrCreateArea(string name)
    {
        var area = Areas.FirstOrDefault(x => x.Name == name);

        if (area == null)
        {
            area = new AreaNode
            {
                Name = name,
            };

            Areas.Add(area);
        }

        return area;
    }
}
