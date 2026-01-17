using Godot;
using System.Collections.Generic;

public partial class MainView : Control
{
    [Export]
    public ScrollPopupMenu PopupMenu;

    public static MainView Instance { get; private set; }

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
        CallDeferred(nameof(Initialize));
    }

    private void Initialize()
    {
    }

    public void RightClickGateNode(GateNodeObject node)
    {
        PopupMenu.ClearItems();

        var gates = GateController.Instance.Gates;
        foreach (var gate in gates)
        {
            if (string.IsNullOrEmpty(gate.Id)) continue;
            if (MainScene.Instance.IsGateCreated(gate)) continue;

            var gate_to_create = gate;
            PopupMenu.AddActionItem(gate_to_create.Name, () => MainScene.Instance.CreateGateConnection(node, gate_to_create));
        }

        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
    }

    public void RightClickAreaNode(AreaNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Delete", () => MainScene.Instance.DeleteArea(node.Area));
        PopupMenu.Show();
        PopupMenu.Size = new Vector2I(100, 0);
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
    }
}
