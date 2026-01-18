using Godot;
using System;
using System.Linq;

public partial class MainView : Control
{
    public static MainView Instance { get; private set; }

    [Export]
    public ScrollPopupMenu PopupMenu;

    [Export]
    public SearchListControl SearchList;

    [Export]
    public MousePrompt MousePrompt;

    private GateController Controller => GateController.Instance;
    private MainScene Scene => MainScene.Instance;

    private bool has_mouse_prompt;
    private event Action OnMousePrompt;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        SearchList.Hide();
        EndMousePrompt();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MousePrompt.Position = GetViewport().GetMousePosition() + new Vector2(0, -40);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        base._UnhandledInput(e);

        if (e is InputEventMouseButton button)
        {
            if (!has_mouse_prompt) return;
            if (button.ButtonIndex == MouseButton.Left)
            {
                OnMousePrompt?.Invoke();
                EndMousePrompt();
                GetViewport().SetInputAsHandled();
            }
            else if (button.ButtonIndex == MouseButton.Right)
            {
                EndMousePrompt();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public void RightClickGateNode(GateNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Traverse", () => OpenGateSearch(node));
        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }


    public void RightClickGroupNode(GroupNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Delete", () => { /* TODO */ });
        PopupMenu.Size = new Vector2I(100, 0);
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }

    private void OpenGateSearch(GateNodeObject node)
    {
        SearchList.Clear();
        SearchList.Title = "Select gate";

        var prev_node = node.ConnectedNodes.FirstOrDefault();
        var next_position = Scene.GetNextNodePosition(prev_node, node);

        var gates = Controller.Gates.Values;
        foreach (var gate in gates)
        {
            if (gate.Name == node.NodeName) continue;
            if (!Controller.IsSearchable(gate.Name)) continue;

            var name = gate.Name;
            SearchList.AddItem(name, () => Scene.CreateNode(name, next_position, node));
        }

        SearchList.Show();
    }

    public void OpenStartSearch()
    {
        SearchList.Clear();
        SearchList.Title = "Select start";

        foreach (var gate in Controller.Gates.Values)
        {
            if (!Controller.IsSearchable(gate.Name)) continue;

            var to_create = gate;
            SearchList.AddItem(gate.Name, () => Scene.CreateNodeAtCenter(to_create.Name));
        }

        SearchList.Show();
    }

    public bool HasActiveUI()
    {
        return SearchList.IsVisibleInTree();
    }

    private void StartMousePrompt(string text, Action action)
    {
        MousePrompt.Show();
        MousePrompt.Label.Text = text;
        has_mouse_prompt = true;
        OnMousePrompt = action;
    }

    private void EndMousePrompt()
    {
        has_mouse_prompt = false;
        MousePrompt.Hide();
    }
}
