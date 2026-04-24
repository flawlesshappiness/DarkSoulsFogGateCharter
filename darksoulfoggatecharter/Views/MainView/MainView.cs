using Godot;
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
    public Control Selection;

    private GateController Gate => GateController.Instance;
    private MainScene Scene => MainScene.Instance;
    private NodeController Node => NodeController.Instance;

    private bool has_popup_menu;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        SearchList.Hide();
        SessionSettings.Hide();

        SessionSettings.ConfirmButton.Pressed += SessionSettingsConfirm_Pressed;

        MouseVisibility.Show(nameof(MainView));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
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

    public void EmptySpace_Clicked(Vector3 position)
    {
        if (HasActiveUI()) return;

        if (has_popup_menu)
        {
            has_popup_menu = false;
            return;
        }

        has_popup_menu = true;

        PopupMenu.ClearItems();
        PopupMenu.AddActionItem("Create", () => OpenGateSearch(position));
        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }

    public void GateNode_Clicked(GateNodeObject node)
    {
        if (HasActiveUI()) return;

        if (has_popup_menu)
        {
            has_popup_menu = false;
            return;
        }

        has_popup_menu = true;

        bool show = false;

        PopupMenu.ClearItems();

        if (GateController.Instance.CanTraverse(node.Gate.Name))
        {
            PopupMenu.AddActionItem("Traverse", () => OpenGateSearch(node));
            show = true;
        }
        else if (node.Gate.Type == GateType.LockedDoor && !node.IsFullyConnected)
        {
            PopupMenu.AddActionItem("Unlock", () => NodeController.Instance.TraverseLockedDoor(node.Gate.Name));
            show = true;
        }

        if (!show) return;

        PopupMenu.Show();
        PopupMenu.Position = (Vector2I)GetViewport().GetMousePosition();
        PopupMenu.Popup();
    }

    /// <summary>
    /// Opens gate search without a connection to a previous node. Only uncreated nodes are listed.
    /// </summary>
    private void OpenGateSearch(Vector3 position)
    {
        var gates = Gate.Gates.Values
            .Where(x => Gate.IsSearchable(x.Name) && !NodeController.Instance.HasNode(x.Name))
            .OrderBy(x => x.Area);

        SearchList.Clear();
        SearchList.Title = "Select gate";
        SearchList.SetGates(gates);
        SearchList.SetAction(gate => Node.StartCreateNode(gate.Name, position, null));
        SearchList.Show();
    }

    /// <summary>
    /// Opens gate search with a connection to the previous node. All valid node connections are listed.
    /// </summary>
    private void OpenGateSearch(GateNodeObject node)
    {
        var next_position = Node.GetNextNodePosition(node);
        var gates = Gate.Gates.Values
            .Where(x => x.Name != node.NodeName && Gate.IsSearchable(x.Name))
            .OrderBy(x => x.Area);

        SearchList.Clear();
        SearchList.Title = "Select gate";
        SearchList.SetGates(gates);
        SearchList.SetAction(gate => Node.StartCreateNode(gate.Name, next_position, node));
        SearchList.Show();
    }

    public void OpenGateList()
    {
        var gates = Gate.Gates.Values
            .Where(x => Gate.IsSearchable(x.Name))
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
        Node.Load(SessionSettings.CreateData());
        OpenStartSearch();
    }

    private void OpenStartSearch()
    {
        var gates = Gate.Gates.Values
            .Where(x => Gate.IsSearchable(x.Name, from_new: true))
            .OrderBy(x => x.Area);

        SearchList.Clear();
        SearchList.Title = "Select start";
        SearchList.SetGates(gates);
        SearchList.SetAction(gate => CreateStart(gate.Name));
        SearchList.Show();
    }

    private void CreateStart(string name)
    {
        Node.CreateNodeAtCenter(name);
        UndoController.Instance.Clear();
    }

    public bool HasActiveUI()
    {
        return SearchList.IsVisibleInTree() || has_popup_menu;
    }
}
