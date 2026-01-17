using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MainScene : Node3D
{
    [Export]
    public PackedScene MainViewPrefab;

    [Export]
    public Node3D NodeParent;

    [Export]
    public GateNodeObject GateNodeTemplate;

    [Export]
    public AreaNodeObject AreaNodeTemplate;

    [Export]
    public ConnectionObject ConnectionObjectTemplate;

    public static MainScene Instance { get; private set; }

    private List<GateNodeObject> gate_nodes = new();
    private List<AreaNodeObject> area_nodes = new();
    private List<ConnectionObject> connections = new();

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        GateNodeTemplate.Hide();
        AreaNodeTemplate.Hide();
        ConnectionObjectTemplate.Hide();

        InitializeMainView();
        CallDeferred("Initialize");
    }

    private void InitializeMainView()
    {
        var view = MainViewPrefab.Instantiate<MainView>();
        GetTree().Root.CallDeferred("add_child", view);
    }

    private void Initialize()
    {
        CreateArea("Asylum Start");
    }

    private void GateRightClick(GateNodeObject gate_node)
    {
        if (IsConnected(gate_node)) return;

        MainView.Instance.RightClickGateNode(gate_node);
    }

    private GateNodeObject CreateGateNode(GateNode gate)
    {
        var node = GateNodeTemplate.Duplicate() as GateNodeObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetGate(gate);
        gate_nodes.Add(node);

        node.OnRightClick += () => GateRightClick(node);

        return node;
    }

    private GateNodeObject GetOrCreateGateNode(GateNode gate)
    {
        return gate_nodes.FirstOrDefault(x => x.Gate == gate) ?? CreateGateNode(gate);
    }

    public bool IsGateCreated(GateNode gate)
    {
        return gate_nodes.Any(x => x.Gate == gate);
    }

    public void CreateGateConnection(GateNodeObject from, GateNode to)
    {
        var area = GateController.Instance.GetOrCreateArea(to.Area);
        CreateArea(area.Name);

        var to_node = GetOrCreateGateNode(to);
        CreateConnectionObject(from, to_node);
    }

    private void AreaRightClick(AreaNodeObject area_node)
    {
        MainView.Instance.RightClickAreaNode(area_node);
    }

    private AreaNodeObject CreateAreaNode(AreaNode area)
    {
        var node = AreaNodeTemplate.Duplicate() as AreaNodeObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetArea(area);
        area_nodes.Add(node);

        node.OnRightClick += () => AreaRightClick(node);

        return node;
    }

    private AreaNodeObject GetOrCreateAreaNode(AreaNode area)
    {
        return area_nodes.FirstOrDefault(x => x.Area == area) ?? CreateAreaNode(area);
    }

    public AreaNodeObject CreateArea(string name)
    {
        var center = DraggableCamera.Instance.ViewportWorldPosition;
        var area = GateController.Instance.GetOrCreateArea(name);
        var area_node = GetOrCreateAreaNode(area);
        area_node.GlobalPosition = center;

        for (int i = 0; i < area.Gates.Count; i++)
        {
            var gate = area.Gates[i];
            var gate_node = GetOrCreateGateNode(gate);
            gate_node.GlobalPosition = area_node.GlobalPosition + GetCirclePosition(i, area.Gates.Count) * 1.5f;

            CreateConnectionObject(area_node, gate_node);
        }

        return area_node;
    }

    public void DeleteArea(AreaNode area)
    {
        var area_node = GetOrCreateAreaNode(area);
        var area_gate_nodes = gate_nodes.Where(x => x.Gate.Area == area.Name).ToList();
        var area_connections = connections.Where(x => area_gate_nodes.Any(y => x.ConnectedObjectA == y || x.ConnectedObjectB == y)).ToList();

        foreach (var c in area_connections)
        {
            c.QueueFree();
            connections.Remove(c);
        }

        foreach (var node in area_gate_nodes)
        {
            node.QueueFree();
            gate_nodes.Remove(node);
        }

        area_node.QueueFree();
        area_nodes.Remove(area_node);
    }

    public bool IsAreaCreated(AreaNode area)
    {
        return area_nodes.Any(x => x.Area == area);
    }

    private Vector3 GetCirclePosition(int index, int count)
    {
        count = Mathf.Max(count, 1);
        index = Mathf.Clamp(index, 0, count);
        var deg = ((float)index / count) * 360f;
        return Vector3.Forward.Rotated(Vector3.Up, Mathf.DegToRad(deg));
    }

    private ConnectionObject CreateConnectionObject(NodeObject A, NodeObject B)
    {
        var node = ConnectionObjectTemplate.Duplicate() as ConnectionObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetConnectedObjects(A, B);
        connections.Add(node);
        return node;
    }

    public bool IsConnected(GateNodeObject node)
    {
        return connections.Count(x => x.ConnectedObjectA == node || x.ConnectedObjectB == node) > 1;
    }
}
