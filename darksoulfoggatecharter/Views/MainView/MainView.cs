using Godot;
using System;

public partial class MainView : View
{
    public static MainView Instance { get; private set; }

    [Export]
    public ScrollPopupMenu PopupMenu;

    [Export]
    public SearchListControl SearchList;

    [Export]
    public SessionSettingsControl SessionSettings;

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
        SessionSettings.Hide();
        EndMousePrompt();

        SessionSettings.ConfirmButton.Pressed += SessionSettingsConfirm_Pressed;

        MouseVisibility.Show(nameof(MainView));
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

    protected override void OnShow()
    {
        base.OnShow();
        MouseVisibility.Show(nameof(MainView));
    }

    protected override void OnHide()
    {
        base.OnHide();
        MouseVisibility.Hide(nameof(MainView));
    }

    public void RightClickGateNode(GateNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Traverse", () => OpenGateSearch(node));
        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }

    public void RightClickObjectiveNode(GateNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Complete", () => Scene.CompleteObjective(node.Gate.Name));
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

        var next_position = Scene.GetNextNodePosition(node);
        var gates = Controller.Gates.Values;
        foreach (var gate in gates)
        {
            if (gate.Name == node.NodeName) continue;
            if (!Controller.IsSearchable(gate.Name)) continue;

            var name = gate.Name;
            SearchList.AddItem(name, () => Scene.StartCreateNode(name, next_position, node));
        }

        SearchList.Show();
    }

    public void OpenStartSettings()
    {
        SessionSettings.Show();
    }

    private void SessionSettingsConfirm_Pressed()
    {
        SessionSettings.Hide();
        OpenStartSearch();
    }

    private void OpenStartSearch()
    {
        SearchList.Clear();
        SearchList.Title = "Select start";

        foreach (var gate in Controller.Gates.Values)
        {
            var name = gate.Name;
            if (!Controller.IsSearchable(name, from_new: true)) continue;

            SearchList.AddItem(name, () => CreateStart(name));
        }

        SearchList.Show();
    }

    private void CreateStart(string name)
    {
        var data = SessionSettings.CreateData();
        Scene.Load(data);
        Scene.CreateNodeAtCenter(name);

        UndoController.Instance.ConfirmUndoAction();
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
