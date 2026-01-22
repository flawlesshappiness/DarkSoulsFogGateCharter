using Godot;

[GlobalClass]
public partial class NodeInfo : Resource
{
    private static NodeInfo _instance;
    public static NodeInfo Instance => _instance ?? (_instance = GD.Load<NodeInfo>($"Node/Resources/{nameof(NodeInfo)}.tres"));

    [Export]
    public PackedScene GateNodePrefab;

    [Export]
    public PackedScene GroupNodePrefab;

    [Export]
    public PackedScene ConnectionPrefab;
}
