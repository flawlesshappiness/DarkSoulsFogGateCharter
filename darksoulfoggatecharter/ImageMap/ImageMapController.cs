using FlawLizArt.Log;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ImageMapController : ResourceController<ImageMapCollection, ImageMapInfo>
{
    public override string Directory => "ImageMap";
    public static ImageMapController Instance => Singleton.Get<ImageMapController>();

    public bool ImageMapModeEnabled { get; private set; }
    public List<ImageMapNode> MapNodes { get; set; } = new();

    public event Action<bool> OnImageMapModeChanged;

    public void Clear()
    {
        foreach (var node in MapNodes)
        {
            node.QueueFree();
        }
        MapNodes.Clear();
    }

    public void CreateMapNodes()
    {
        Clear();
        foreach (var info in Collection.Resources)
        {
            var node = NodeController.Instance.CreateOtherNode<ImageMapNode>(info.Prefab);
            node.Hide();
            MapNodes.Add(node);
            Log.Trace($"Created ImageMapNode: {node.NodeName}");
        }
    }

    public void SetImageMapModeEnabled(bool enabled)
    {
        if (enabled)
        {
            foreach (var node in NodeController.Instance.GetNodes())
            {
                var imn = GetMapNodeFromGate(node.NodeName);
                if (imn == null) continue;
                imn.Show();
            }
        }
        else
        {
            foreach (var imn in MapNodes)
            {
                imn.Hide();
            }
        }

        UndoController.Instance.AddImageMapModeAction(enabled);
        ImageMapModeEnabled = enabled;
        OnImageMapModeChanged?.Invoke(enabled);
    }

    private ImageMapNode GetMapNodeFromGate(string name)
    {
        foreach (var node in MapNodes)
        {
            if (node.MapMarkers.Any(x => x.GateName == name))
            {
                return node;
            }
        }

        return null;
    }

    public ImageMapMarker GetMapMarker(string name)
    {
        foreach (var node in MapNodes)
        {
            var marker = node.MapMarkers.FirstOrDefault(x => x.GateName == name);
            if (marker != null)
            {
                return marker;
            }
        }

        return null;
    }
}
