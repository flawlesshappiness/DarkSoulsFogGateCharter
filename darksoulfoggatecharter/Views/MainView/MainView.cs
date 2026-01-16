using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MainView : Control
{
    public static MainView Instance { get; private set; }

    [Export(PropertyHint.File)]
    public string GatesPath;

    [Export]
    public GateGraph GateGraph;

    private bool initialized;
    public List<Gate> Gates { get; private set; } = new();
    public List<string> Areas { get; private set; } = new();
    public List<AreaGraphNode> Nodes { get; private set; } = new();

    public class Gate
    {
        public string Area { get; set; }
        public string Description { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        ReadGatesFile();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        CreateGateNodes();
    }

    private void ReadGatesFile()
    {
        var file = FileAccess.GetFileAsString(GatesPath);
        var lines = file.Split("\n");

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split(',');
            var gate = new Gate
            {
                Area = data[0],
                Description = data[1]
            };

            Gates.Add(gate);
            AddArea(gate.Area);
        }
    }

    private void AddArea(string area)
    {
        if (Areas.Contains(area)) return;
        Areas.Add(area);
    }

    private void CreateGateNodes()
    {
        Nodes.Clear();

        foreach (var gate in Gates)
        {
            var node = Nodes.FirstOrDefault(x => x.Title == gate.Area);
            node ??= GateGraph.CreateNode();
            node.SetArea(gate.Area);
            node.AddGate(gate.Description);
            Nodes.Add(node);
        }
    }
}
