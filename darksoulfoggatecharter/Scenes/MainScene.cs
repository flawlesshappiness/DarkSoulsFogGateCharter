using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MainScene : Node3D
{
    public static MainScene Instance { get; private set; }

    [Export]
    public PackedScene MainViewPrefab;

    [Export]
    public Node3D NodeParent;

    [Export]
    public GateNodeObject GateNodeTemplate;

    [Export]
    public GroupNodeObject GroupNodeTemplate;

    [Export]
    public ConnectionObject ConnectionObjectTemplate;

    public const float DEFAULT_NODE_DISTANCE = 1.5f;

    private GateController Controller => GateController.Instance;
    private MainView View => MainView.Instance;
    private DraggableCamera Camera => DraggableCamera.Instance;

    private List<ConnectionObject> connections = new();
    private Dictionary<string, NodeObject> nodes = new();

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        GateNodeTemplate.Hide();
        GroupNodeTemplate.Hide();
        ConnectionObjectTemplate.Hide();

        InitializeMainView();
        CallDeferred(nameof(CreateStart));
    }

    private void InitializeMainView()
    {
        var view = MainViewPrefab.Instantiate<MainView>();
        GetTree().Root.CallDeferred("add_child", view);
    }

    public void Clear()
    {
        foreach (var connection in connections)
        {
            connection.QueueFree();
        }
        connections.Clear();

        foreach (var node in nodes.Values)
        {
            node.QueueFree();
        }
        nodes.Clear();

        Controller.ClearData();
    }

    private void CreateStart()
    {
        View.OpenStartSearch();
    }

    // GATE //
    private GateNodeObject CreateGateNode(GateNode gate)
    {
        var node = GateNodeTemplate.Duplicate() as GateNodeObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetGate(gate);
        nodes.Add(gate.Name, node);

        node.OnRightClick += () => GateRightClick(node);

        return node;
    }

    private void GateRightClick(GateNodeObject node)
    {
        if (node.IsFullyConnected) return;
        MainView.Instance.RightClickGateNode(node);
    }

    // GROUP //
    private GroupNodeObject CreateGroupNode(GateGroup group)
    {
        var node = GroupNodeTemplate.Duplicate() as GroupNodeObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetGroup(group);
        nodes.Add(group.Name, node);

        //node.OnRightClick += () => AreaRightClick(node);

        return node;
    }

    // CONNECTION //
    private ConnectionObject ConnectNodes(NodeObject A, NodeObject B)
    {
        if (A == null) return null;
        if (B == null) return null;
        if (A.IsConnectedTo(B)) return null;

        A.AddConnection(B);
        B.AddConnection(A);

        var node = CreateConnectionObject();
        node.SetConnectedObjects(A, B);
        return node;
    }

    private ConnectionObject CreateConnectionObject()
    {
        var node = ConnectionObjectTemplate.Duplicate() as ConnectionObject;
        NodeParent.AddChild(node);
        node.Show();
        connections.Add(node);
        return node;
    }

    // NODES //
    private void TryCreateNext(string name, Vector3 position)
    {
        if (Controller.IsGroup(name))
        {
            var group = Controller.GetGroup(name);
            var node = GetNode(name);
            var dir = node.GlobalPosition.DirectionTo(position);
            var existing_node = group.Gates.Values.FirstOrDefault(x => nodes.ContainsKey(x.Name));
            var nodes_to_create = group.Gates.Values.OrderBy(x => x != existing_node).ToList();

            var start_position = node.GlobalPosition;
            var count = nodes_to_create.Count;
            for (int i = 0; i < count; i++)
            {
                var node_to_create = nodes_to_create[i];
                var next_is_group = Controller.IsGroup(node_to_create.Name);
                var mul = next_is_group ? 2 : 1;
                var next_position = start_position + GetCirclePosition(i, count, dir) * DEFAULT_NODE_DISTANCE * mul;
                CreateNode(node_to_create.Name, next_position, node);
            }
        }
        else
        {
            var gate = Controller.GetGate(name);
            var node = GetNode(name);
            var next = gate.Connection;
            var should_generate = Controller.ShouldAutoGenerate(next);
            var is_one_way = Controller.IsOneWay(next);

            if (should_generate || is_one_way)
            {
                CreateNode(next, position, node);
            }
        }
    }

    public NodeObject CreateNode(string name, Vector3 position, NodeObject node_prev = null)
    {
        if (Controller.IsGroup(name))
        {
            var group = Controller.GetGroup(name);
            if (HasNode(name))
            {
                var node = GetNode(name);
                ConnectNodes(node_prev, node);
                return node;
            }
            else
            {
                var node = CreateGroupNode(group);
                node.GlobalPosition = position;
                ConnectNodes(node_prev, node);

                var next_position = node_prev?.GlobalPosition ?? Vector3.Right * DEFAULT_NODE_DISTANCE;
                TryCreateNext(name, next_position);
                return node;
            }
        }
        else
        {
            var gate = Controller.GetGate(name);
            if (HasNode(name))
            {
                var node = GetNode(name);
                ConnectNodes(node_prev, node);
                return node;
            }
            else
            {
                var node = CreateGateNode(gate);
                node.GlobalPosition = position;
                ConnectNodes(node_prev, node);

                var next_position = GetNextNodePosition(node_prev, node);
                TryCreateNext(name, next_position);

                return node;
            }
        }
    }

    public NodeObject CreateNodeAtCenter(string name, NodeObject node_prev = null) =>
        CreateNode(name, Camera.ViewportWorldPosition, node_prev);

    public bool HasNode(string name) =>
        nodes.ContainsKey(name);

    public NodeObject GetNode(string name) =>
        nodes[name];

    public bool IsNodeFullyConnected(string name)
    {
        if (!HasNode(name)) return false;
        var node = GetNode(name);
        return node.IsFullyConnected;
    }

    public Vector3 GetNextNodePosition(NodeObject previous, NodeObject current)
    {
        var dir = previous == null ? Vector3.Right : previous.GlobalPosition.DirectionTo(current.GlobalPosition);
        return current.GlobalPosition + dir * DEFAULT_NODE_DISTANCE;
    }

    private Vector3 GetCirclePosition(int index, int count, Vector3? start_dir = null)
    {
        count = Mathf.Max(count, 1);
        index = Mathf.Clamp(index, 0, count);
        var deg = ((float)index / count) * 360f;
        var direction = start_dir ?? Vector3.Forward;
        return direction.Rotated(Vector3.Up, Mathf.DegToRad(deg));
    }

    // DATA //
    public SaveData GenerateSaveData()
    {
        var save_data = new SaveData();

        foreach (var gate in Controller.Gates.Values)
        {
            if (!HasNode(gate.Name)) continue;

            var node = GetNode(gate.Name);
            var p = node.GlobalPosition;
            gate.Data.X = p.X;
            gate.Data.Y = p.Y;
            gate.Data.Z = p.Z;
            gate.Data.Name = gate.Name;
            gate.Data.Connections = node.ConnectedNodes.Select(x => x.NodeName).ToList();
            save_data.Gates.Add(gate.Data);
        }

        foreach (var group in Controller.Groups.Values)
        {
            if (!HasNode(group.Name)) continue;

            var data = new GroupData();
            var node = GetNode(group.Name);
            var p = node.GlobalPosition;
            data.X = p.X;
            data.Y = p.Y;
            data.Z = p.Z;
            data.Name = group.Name;
            save_data.Groups.Add(data);
        }

        return save_data;
    }

    public void Load(SaveData save_data)
    {
        Clear();

        foreach (var data in save_data.Gates)
        {
            var gate = Controller.GetGate(data.Name);
            gate.Data = data;

            var position = new Vector3(data.X, data.Y, data.Z);
            var node = CreateNode(data.Name, position);
            node.GlobalPosition = position;

            foreach (var connection in data.Connections)
            {
                if (!HasNode(connection)) continue;
                var other = GetNode(connection);
                ConnectNodes(node, other);
            }
        }

        foreach (var data in save_data.Groups)
        {
            var node = GetNode(data.Name);
            var position = new Vector3(data.X, data.Y, data.Z);
            node.GlobalPosition = position;
        }
    }
}
