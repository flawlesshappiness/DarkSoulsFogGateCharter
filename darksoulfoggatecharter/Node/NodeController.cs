using FlawLizArt.Log;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NodeController : SingletonController
{
    public override string Directory => "Node";
    public static NodeController Instance => Singleton.Get<NodeController>();
    private GateController Gate => GateController.Instance;
    private MainView View => MainView.Instance;
    private DraggableCamera Camera => DraggableCamera.Instance;
    private UndoController Undo => UndoController.Instance;
    public MainScene Scene => MainScene.Instance;
    public bool IsLoaded { get; private set; }

    public const float DEFAULT_NODE_DISTANCE = 1.5f;

    public event Action OnClear;
    public event Action OnNodeChanges;
    public event Action<NodeObject> OnNodeCreated;
    public event Action<NodeObject> OnNodeRemoved;

    private Dictionary<string, ConnectionObject> connections = new();
    private Dictionary<string, NodeObject> nodes = new();

    protected override void Initialize()
    {
        base.Initialize();
        ImageMapController.Instance.CreateMapNodes();
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

        Log.Clear();
        UndoController.Instance.Clear();

        OnClear?.Invoke();
    }

    // GATE //
    private GateNodeObject CreateGateNode(GateNode gate)
    {
        var node = NodeInfo.Instance.GateNodePrefab.Instantiate<GateNodeObject>();
        node.SetParent(Scene.NodeParent);
        node.Show();
        node.Initialize(gate.Name);
        nodes.Add(gate.Name, node);

        node.OnDragEnded += () => Node_DragEnded(node);
        node.OnClicked += () => Gate_Clicked(node);

        OnNodeCreated?.Invoke(node);
        OnNodeChanges?.Invoke();

        return node;
    }

    private void Gate_Clicked(GateNodeObject node)
    {
        View.GateNode_Clicked(node);
    }

    private bool IsConnectedToGroup(NodeObject node)
    {
        if (Gate.IsGroup(node.Name))
        {
            return false;
        }
        else
        {
            foreach (var connected in node.ConnectedNodes.Values)
            {
                if (Gate.IsGroup(connected.Name)) return true;
            }

            return false;
        }
    }

    // OTHER NODE //
    public T CreateOtherNode<T>(PackedScene prefab) where T : NodeObject
    {
        var node = prefab.Instantiate<T>();
        node.SetParent(Scene.NodeParent);
        node.Show();

        node.OnDragEnded += () => Node_DragEnded(node);

        return node;
    }

    // GROUP //
    private GroupNodeObject CreateGroupNode(GateGroup group)
    {
        var node = NodeInfo.Instance.GroupNodePrefab.Instantiate<GroupNodeObject>();
        node.SetParent(Scene.NodeParent);
        node.Show();
        node.Initialize(group.Name);
        nodes.Add(group.Name, node);

        node.OnDragEnded += () => Node_DragEnded(node);

        OnNodeCreated?.Invoke(node);
        OnNodeChanges?.Invoke();

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

        Log.Trace($"Connect: {id}");

        A.AddConnection(id, B);
        B.AddConnection(id, A);

        var node = CreateConnectionObject(id);
        node.SetConnectedObjects(A, B);

        OnNodeChanges?.Invoke();

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
        var node = NodeInfo.Instance.ConnectionPrefab.Instantiate<ConnectionObject>();
        node.SetParent(Scene.NodeParent);
        node.Show();
        connections.Add(id, node);

        UndoController.Instance.AddCreateConnectionAction(id);
        return node;
    }

    public void RemoveConnectionObject(string id)
    {
        var connection = connections.TryGetValue(id, out var result) ? result : null;
        if (connection == null) return;

        Log.Trace($"Disconnect: {id}");
        connection.ObjectA.RemoveConnection(id);
        connection.ObjectB.RemoveConnection(id);
        connection.DestroyConnection();
        connections.Remove(id);

        OnNodeChanges?.Invoke();
    }

    // NODES //
    private void TryCreateNext(string name, Vector3 position)
    {
        if (Gate.IsGroup(name))
        {
            var group = Gate.GetGroup(name);
            var node = GetNode(name);
            var dir = node.GlobalPosition.DirectionTo(position);
            var existing_node = group.Gates.Values.FirstOrDefault(x => HasNode(x.Name));
            var gates_to_create = group.Gates.Values
                .Select(x => Gate.GetNextValidGate(x.Name))
                .Where(x => x != null)
                .Where(x => Gate.IsVisibleInGroup(x))
                .Distinct()
                .ToList();

            if (existing_node != null)
            {
                gates_to_create = gates_to_create
                    .Append(existing_node.Name)
                    .OrderBy(x => x != existing_node.Name)
                    .ToList();
            }

            var start_position = node.GlobalPosition;
            var count = gates_to_create.Count;
            var j = IsConnectedToGroup(node) ? 1 : 0;
            for (int i = 0; i < count; i++)
            {
                var gate = gates_to_create[i];
                var next_is_group = Gate.IsGroup(gate);
                var mul = next_is_group ? 2 : 1;
                var next_position = start_position + GetCirclePosition(i + j, count + j, dir) * DEFAULT_NODE_DISTANCE * mul;
                CreateNode(gate, next_position, node);
            }
        }
        else
        {
            var gate = Gate.GetGate(name);
            var node = GetNode(name);
            var next = Gate.GetNextValidGate(gate.Location);

            if (!string.IsNullOrEmpty(next))
            {
                CreateNode(next, position, node);
            }

            if (Gate.IsDisabled(name))
            {
                if (gate.HasId)
                {
                    var exit = Gate.GetGateExit(name);
                    CreateNode(exit.Name, position, node);
                }

                var dir = node.GlobalPosition.DirectionTo(position);
                var connections = Gate.GetGatesByLocation(name).ToList();
                var count = connections.Count();
                for (int i = 0; i < count; i++)
                {
                    var connection = connections[i];
                    var arc_position = node.GlobalPosition + GetArcPosition(dir, 90, i, count) * DEFAULT_NODE_DISTANCE;
                    CreateNode(connection.Name, arc_position, node);
                }
                foreach (var connection in connections)
                {
                    CreateNode(connection.Name, position, node);
                }
            }
            else if (Gate.IsShortcut(name))
            {
                var exit = Gate.GetGateExit(name);
                CreateNode(exit.Location, position, node);
            }
            else if (gate.Type == GateType.Objective)
            {
                TraverseObjective(name);
            }
            else if (gate.Type == GateType.LockedDoor)
            {
                var other = Gate.GetGateExit(name).Name;
                if (HasNode(other))
                {
                    CreateNode(other, position, node);
                }
            }
            else if (gate.Type == GateType.Area)
            {
                var other = Gate.GetGateExit(name).Name;
                CreateNode(other, position, node);
            }
        }
    }

    public void TraverseObjective(string name)
    {
        //Undo.StartUndoAction("Objective completed"); // Disabled be cause objectives are automatically traversed

        var gate = Gate.GetGate(name);
        var node = GetNode(name);
        var dir = GetNextNodeDirection(node);

        var connections = Gate.GetGatesByLocation(name).ToList();
        var count = connections.Count();
        for (int i = 0; i < count; i++)
        {
            var connection = connections[i];
            var position = node.GlobalPosition + GetArcPosition(dir, 90, i, count) * DEFAULT_NODE_DISTANCE;
            Log.Trace($"Objective: {name} > {connection.Name}");
            CreateNode(connection.Name, position, node);
        }

        //Undo.EndUndoAction();
    }

    public void TraverseLockedDoor(string name)
    {
        var gate = Gate.GetGate(name);
        if (gate.Type != GateType.LockedDoor) return;

        var node = GetNode(name);
        var pos = GetNextNodePosition(node);
        var exit = GateController.Instance.GetGateExit(name);
        var exit_location = GateController.Instance.GetNextValidGate(exit.Location);

        StartCreateNode(exit_location, pos, node);
    }

    public NodeObject StartCreateNode(string name, Vector3 position, NodeObject node_prev = null)
    {
        NodeObject result = null;

        try
        {
            Undo.StartUndoAction($"Create node {name}");
            result = CreateNode(name, position, node_prev);
            Undo.EndUndoAction();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }

        return result;
    }

    public NodeObject CreateNode(string name, Vector3 position, NodeObject node_prev = null)
    {
        if (Gate.IsGroup(name))
        {
            var group = Gate.GetGroup(name);
            if (HasNode(name))
            {
                var node = GetNode(name);
                ConnectNodes(node_prev, node);
                return node;
            }
            else
            {
                Log.Trace($"Create group: {name}");
                var node = CreateGroupNode(group);
                node.GlobalPosition = position;

                UndoController.Instance.AddCreateNodeAction(group.Name, position, node_prev);

                ConnectNodes(node_prev, node);

                var next_position = node_prev?.GlobalPosition ?? Vector3.Right * DEFAULT_NODE_DISTANCE;
                TryCreateNext(name, next_position);
                return node;
            }
        }
        else
        {
            var gate = Gate.GetGate(name);
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
            else if (gate.Type == GateType.LockedDoor && HasNode(Gate.GetGateExit(name).Name))
            {
                return CreateNode(Gate.GetGateExit(name).Name, position, node_prev);
            }
            else
            {
                Log.Trace($"Create node: {name}");
                var node = CreateGateNode(gate);
                node.GlobalPosition = position;

                UndoController.Instance.AddCreateNodeAction(gate.Name, position, node_prev);

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
        Log.Trace($"Remove: {name}");
        var node = GetNode(name);
        OnNodeRemoved?.Invoke(node);
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
        Undo.StartUndoAction($"Dragged node {node.NodeName}");
        Undo.AddMoveNodeAction(node.NodeName, node.DragStartPosition, node.DragEndPosition);
        Undo.EndUndoAction();

        OnNodeChanges?.Invoke();
    }

    public bool HasNode(string name) =>
        nodes.ContainsKey(name);

    public NodeObject GetNode(string name) =>
        nodes[name];

    public List<NodeObject> GetNodes() =>
        nodes.Values.ToList();

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
        save_data.DisabledTypes = Gate.DisabledTypes.ToList();

        foreach (var gate in Gate.Gates.Values)
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

        foreach (var group in Gate.Groups.Values)
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
        Gate.LoadSettings(save_data);

        foreach (var data in save_data.Gates)
        {
            var gate = Gate.GetGate(data.Name);
            var position = new Vector3(data.X, data.Y, data.Z);
            var node = CreateNode(data.Name, position);

            if (node == null)
            {
                Log.Warning($"Failed to load node: {data.Name}");
                continue;
            }

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

        IsLoaded = true;
    }
}
