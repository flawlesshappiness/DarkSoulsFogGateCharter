using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MainScene : Scene
{
    public static MainScene Instance { get; private set; }

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
    private UndoController Undo => UndoController.Instance;

    private Dictionary<string, ConnectionObject> connections = new();
    private Dictionary<string, NodeObject> nodes = new();

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        GateNodeTemplate.Hide();
        GroupNodeTemplate.Hide();
        ConnectionObjectTemplate.Hide();

        CallDeferred(nameof(CreateStart));
    }

    public void Clear()
    {
        foreach (var connection in connections.Values)
        {
            connection.QueueFree();
        }
        connections.Clear();

        foreach (var node in nodes.Values)
        {
            node.QueueFree();
        }
        nodes.Clear();

        UndoController.Instance.Clear();
    }

    private void CreateStart()
    {
        View.Show();
    }

    // GATE //
    private GateNodeObject CreateGateNode(GateNode gate)
    {
        var node = GateNodeTemplate.Duplicate() as GateNodeObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetGate(gate);
        nodes.Add(gate.Name, node);

        node.OnDragEnded += () => Node_DragEnded(node);
        node.OnRightClick += () => GateRightClick(node);

        return node;
    }

    private void GateRightClick(GateNodeObject node)
    {
        if (node.IsFullyConnected) return;

        if (node.Gate.Type == GateType.Objective)
        {
            View.RightClickObjectiveNode(node);
        }
        else
        {
            View.RightClickGateNode(node);
        }
    }

    private bool IsConnectedToGroup(NodeObject node)
    {
        if (Controller.IsGroup(node.Name))
        {
            return false;
        }
        else
        {
            foreach (var connected in node.ConnectedNodes.Values)
            {
                if (Controller.IsGroup(connected.Name)) return true;
            }

            return false;
        }
    }

    // GROUP //
    private GroupNodeObject CreateGroupNode(GateGroup group)
    {
        var node = GroupNodeTemplate.Duplicate() as GroupNodeObject;
        NodeParent.AddChild(node);
        node.Show();
        node.SetGroup(group);
        nodes.Add(group.Name, node);

        node.OnDragEnded += () => Node_DragEnded(node);

        return node;
    }

    // CONNECTION //
    private ConnectionObject ConnectNodes(NodeObject A, NodeObject B)
    {
        if (A == null) return null;
        if (B == null) return null;

        var ids = new List<string>() { A.NodeName, B.NodeName }.OrderBy(x => x).ToList();
        var id = $"{ids[0]},{ids[1]}";
        if (A.HasConnection(id)) return null;
        if (B.HasConnection(id)) return null;

        Debug.LogMethod($"{id}");

        A.AddConnection(id, B);
        B.AddConnection(id, A);

        var node = CreateConnectionObject(id);
        node.SetConnectedObjects(A, B);
        return node;
    }

    public ConnectionObject ConnectNodes(string connection_id)
    {
        var split = connection_id.Split(',');
        var name_a = split[0];
        var name_b = split[1];
        var a = GetNode(name_a);
        var b = GetNode(name_b);
        return ConnectNodes(a, b);
    }

    private ConnectionObject CreateConnectionObject(string id)
    {
        var node = ConnectionObjectTemplate.Duplicate() as ConnectionObject;
        NodeParent.AddChild(node);
        node.Show();
        connections.Add(id, node);

        UndoController.Instance.AddCreateConnectionAction(id);

        return node;
    }

    public void RemoveConnectionObject(string id)
    {
        var connection = connections.TryGetValue(id, out var result) ? result : null;
        if (connection == null) return;

        Debug.LogMethod(id);
        connection.ObjectA.RemoveConnection(id);
        connection.ObjectB.RemoveConnection(id);
        connection.QueueFree();
        connections.Remove(id);
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
            var nodes_to_create = group.Gates.Values
                .Where(x => Controller.ShowInGroup(x.Name))
                .Append(existing_node) // Not sure this is the best way to do it
                .Distinct()
                .OrderBy(x => x != existing_node).ToList();

            var start_position = node.GlobalPosition;
            var count = nodes_to_create.Count;
            var j = IsConnectedToGroup(node) ? 1 : 0;
            for (int i = 0; i < count; i++)
            {
                var node_to_create = nodes_to_create[i];
                if (node_to_create == null) continue; // This could be an issue

                var next_is_group = Controller.IsGroup(node_to_create.Name);
                var mul = next_is_group ? 2 : 1;
                var next_position = start_position + GetCirclePosition(i + j, count + j, dir) * DEFAULT_NODE_DISTANCE * mul;
                CreateNode(node_to_create.Name, next_position, node);
            }
        }
        else
        {
            var gate = Controller.GetGate(name);
            var node = GetNode(name);

            CreateNode(gate.Location, position, node);

            if (Controller.IsDisabled(name))
            {
                if (!string.IsNullOrEmpty(gate.Id))
                {
                    var exit = Controller.GetGateExit(name);
                    GD.Print($"{gate.Type} Disabled: {name} > {exit.Name}");
                    CreateNode(exit.Name, position, node);
                }

                var dir = node.GlobalPosition.DirectionTo(position);
                var connections = Controller.GetGatesByLocation(name).ToList();
                var count = connections.Count();
                for (int i = 0; i < count; i++)
                {
                    var connection = connections[i];
                    var arc_position = node.GlobalPosition + GetArcPosition(dir, 90, i, count) * DEFAULT_NODE_DISTANCE;
                    GD.Print($"Objective: {name} > {connection.Name}");
                    CreateNode(connection.Name, arc_position, node);
                }
                foreach (var connection in connections)
                {
                    GD.Print($"{gate.Type} Disabled: {name} > {connection.Name}");
                    CreateNode(connection.Name, position, node);
                }
            }
            else if (Controller.IsShortcut(name))
            {
                var exit = Controller.GetGateExit(name);
                var next = exit.Location;
                GD.Print($"Shortcut: {name} > {next}");
                CreateNode(next, position, node);
            }
        }
    }

    public void CompleteObjective(string name)
    {
        Undo.StartUndoAction();

        var gate = Controller.GetGate(name);
        var node = GetNode(name);
        var dir = GetNextNodeDirection(node);

        var connections = Controller.GetGatesByLocation(name).ToList();
        var count = connections.Count();
        for (int i = 0; i < count; i++)
        {
            var connection = connections[i];
            var position = node.GlobalPosition + GetArcPosition(dir, 90, i, count) * DEFAULT_NODE_DISTANCE;
            GD.Print($"Objective: {name} > {connection.Name}");
            CreateNode(connection.Name, position, node);
        }

        Undo.EndUndoAction();
    }

    public NodeObject StartCreateNode(string name, Vector3 position, NodeObject node_prev = null)
    {
        Undo.StartUndoAction();
        var result = CreateNode(name, position, node_prev);
        Undo.EndUndoAction();

        return result;
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
                GD.Print($"Create group: {name}");
                var node = CreateGroupNode(group);
                node.GlobalPosition = position;

                UndoController.Instance.AddCreateNodeAction(group.Name, position);

                ConnectNodes(node_prev, node);

                var next_position = node_prev?.GlobalPosition ?? Vector3.Right * DEFAULT_NODE_DISTANCE;
                TryCreateNext(name, next_position);
                return node;
            }
        }
        else
        {
            var gate = Controller.GetGate(name);
            if (gate == null)
            {
                return null;
            }
            else if (HasNode(name))
            {
                var node = GetNode(name);
                ConnectNodes(node_prev, node);
                return node;
            }
            else
            {
                GD.Print($"Create node: {name}");
                var node = CreateGateNode(gate);
                node.GlobalPosition = position;

                UndoController.Instance.AddCreateNodeAction(gate.Name, position);

                ConnectNodes(node_prev, node);

                var next_position = GetNextNodePosition(node, node_prev);
                TryCreateNext(name, next_position);

                return node;
            }
        }
    }

    public NodeObject CreateNodeAtCenter(string name, NodeObject node_prev = null) =>
        CreateNode(name, Camera.ViewportWorldPosition, node_prev);

    public void RemoveNode(string name)
    {
        Debug.LogMethod(name);
        var node = GetNode(name);
        node?.DestroyNode();
        nodes.Remove(name);
    }

    public void MoveNode(string name, Vector3 position)
    {
        var node = GetNode(name);
        node?.SetGlobalPosition(position);
    }

    public void Node_DragEnded(NodeObject node)
    {
        Undo.StartUndoAction();
        Undo.AddMoveNodeAction(node.NodeName, node.DragStartPosition, node.DragEndPosition);
        Undo.EndUndoAction();
    }

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

    private Vector3 GetNextNodeDirection(NodeObject current, NodeObject previous = null)
    {
        previous ??= current.ConnectedNodes.Values.FirstOrDefault();
        var dir = previous == null ? Vector3.Right : previous.GlobalPosition.DirectionTo(current.GlobalPosition);
        return dir;
    }

    public Vector3 GetNextNodePosition(NodeObject current, NodeObject previous = null)
    {
        var dir = GetNextNodeDirection(current, previous);
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

    private Vector3 GetArcPosition(Vector3 dir, float arc, int index, int count)
    {
        count = Mathf.Max(count - 1, 1);
        index = Mathf.Clamp(index, 0, count);
        arc = Mathf.Max(arc, 1);

        var deg = -((float)index / count) * arc;
        var deg_start = arc * 0.5f;
        var start = dir.Rotated(Vector3.Up, Mathf.DegToRad(deg_start));

        return start.Rotated(Vector3.Up, Mathf.DegToRad(deg));
    }

    // DATA //
    public SessionData GenerateSaveData()
    {
        var save_data = new SessionData();
        save_data.Update();
        save_data.DisabledTypes = Controller.DisabledTypes.ToList();

        foreach (var gate in Controller.Gates.Values)
        {
            if (!HasNode(gate.Name)) continue;

            var node = GetNode(gate.Name);
            var p = node.GlobalPosition;
            var connections = node.ConnectedNodes.Values.Select(x => x.NodeName).ToList();
            var data = new GateData
            {
                Name = gate.Name,
                X = p.X,
                Y = p.Y,
                Z = p.Z,
                Connections = connections
            };

            save_data.Gates.Add(data);
        }

        foreach (var group in Controller.Groups.Values)
        {
            if (!HasNode(group.Name)) continue;

            var node = GetNode(group.Name);
            var p = node.GlobalPosition;
            var data = new GroupData
            {
                Name = group.Name,
                X = p.X,
                Y = p.Y,
                Z = p.Z,
            };

            save_data.Groups.Add(data);
        }

        return save_data;
    }

    public void Load(SessionData save_data)
    {
        Clear();
        Controller.LoadSettings(save_data);

        foreach (var data in save_data.Gates)
        {
            var gate = Controller.GetGate(data.Name);
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
