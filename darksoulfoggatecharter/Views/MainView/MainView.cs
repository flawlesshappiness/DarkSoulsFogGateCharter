using Godot;
using System;
using System.Linq;

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

    [Export]
    public Control Selection;

    private GateController Controller => GateController.Instance;
    private MainScene Scene => MainScene.Instance;
    private NodeController Node => NodeController.Instance;

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
        Process_Selection();
    }

    private void Process_Selection()
    {
        Selection.Visible = SelectionController.Instance.Dragging;
        var a = SelectionController.Instance.ViewportDragStart;
        var b = DraggableCamera.Instance.MousePosition;
        var lx = Mathf.Min(a.X, b.X);
        var hx = Mathf.Max(a.X, b.X);
        var ly = Mathf.Min(a.Y, b.Y);
        var hy = Mathf.Max(a.Y, b.Y);
        var position = new Vector2(lx, ly);
        var size = new Vector2(hx - lx, hy - ly);
        Selection.Position = position;
        Selection.Size = size;
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

    public void GateNode_Clicked(GateNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Traverse", () => OpenGateSearch(node));
        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }

    public void Objective_Clicked(GateNodeObject node)
    {
        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Complete", () => Node.CompleteObjective(node.Gate.Name));
        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }

    private void OpenGateSearch(GateNodeObject node)
    {
        var next_position = Node.GetNextNodePosition(node);
        var gates = Controller.Gates.Values
            .Where(x => x.Name != node.NodeName && Controller.IsSearchable(x.Name))
            .OrderBy(x => x.Area);

        SearchList.Clear();
        SearchList.Title = "Select gate";
        SearchList.SetGates(gates);
        SearchList.SetAction(gate => Node.StartCreateNode(gate.Name, next_position, node));
        SearchList.Show();
    }

    public void OpenGateList()
    {
        var gates = Controller.Gates.Values
            .Where(x => Controller.IsSearchable(x.Name))
            .OrderBy(x => x.Area);

        SearchList.Clear();
        SearchList.Title = "All gates";
        SearchList.SetGates(gates);
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
        var gates = Controller.Gates.Values
            .Where(x => Controller.IsSearchable(x.Name, from_new: true))
            .OrderBy(x => x.Area);

        SearchList.Clear();
        SearchList.Title = "Select start";
        SearchList.SetGates(gates);
        SearchList.SetAction(gate => CreateStart(gate.Name));
        SearchList.Show();
    }

    private void CreateStart(string name)
    {
        var data = SessionSettings.CreateData();
        Node.Load(data);
        Node.CreateNodeAtCenter(name);

        UndoController.Instance.Clear();
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
