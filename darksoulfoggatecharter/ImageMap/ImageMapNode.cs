using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ImageMapNode : NodeObject
{
    [Export]
    public string AreaName;

    public List<ImageMapMarker> MapMarkers { get; set; } = new();
    public override string NodeName => AreaName;

    public override void _Ready()
    {
        base._Ready();
        InitializeMarkers();
    }

    private void InitializeMarkers()
    {
        MapMarkers = this.GetNodesInChildren<ImageMapMarker>();
    }

    protected override void InitializeOtherNodes()
    {
        foreach (var node in ImageMapController.Instance.MapNodes)
        {
            if (node == this) continue;
            var relation = GetOrCreateRelation(node);
            relation.MinDistance = 10;
        }
    }

    public ImageMapMarker GetMarker(string name)
    {
        return MapMarkers.FirstOrDefault(x => x.Name == name);
    }
}
