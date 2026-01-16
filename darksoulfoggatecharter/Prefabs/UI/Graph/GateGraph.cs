using Godot;

public partial class GateGraph : GraphEdit
{
    [Export]
    public AreaGraphNode AreaNodeTemplate;

    public override void _Ready()
    {
        base._Ready();
        AreaNodeTemplate.Hide();
        ConnectionRequest += _ConnectionRequest;
        DisconnectionRequest += _DisconnectionRequest;
    }

    private void _ConnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
    }

    private void _DisconnectionRequest(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
    }

    public AreaGraphNode CreateNode()
    {
        var node = AreaNodeTemplate.Duplicate() as AreaGraphNode;
        AreaNodeTemplate.GetParent().AddChild(node);
        node.Show();
        return node;
    }
}
