using Godot;

public partial class SearchBar : MarginContainer
{
    [Export]
    public TextEdit TextEdit;

    [Export]
    public Button SearchButton;

    [Export]
    public Button OpenGateButton;

    private bool open_gate_toggled;

    public override void _Ready()
    {
        base._Ready();
        TextEdit.TextChanged += TextEdit_TextChanged;
        OpenGateButton.Pressed += OpenGateButton_Pressed;
        InputController.Instance.OnNodeClicked += _ => ClearFocus();
        InputController.Instance.OnEmptySpaceClicked += ClearFocus;
        InputController.Instance.OnDrag += ClearFocus;
        InputController.Instance.OnShortcutSearch += Shortcut_Search;

        OpenGateButton_ToggleChanged();
    }

    private void TextEdit_TextChanged()
    {
        var text = TextEdit.Text;
        SearchController.Instance.SetSearchTerm(text);
    }

    private void ClearFocus()
    {
        if (TextEdit.HasFocus())
        {
            TextEdit.ReleaseFocus();
        }
    }

    private void Shortcut_Search()
    {
        TextEdit.GrabFocus();
        TextEdit.SelectAll();
    }

    private void OpenGateButton_Pressed()
    {
        open_gate_toggled = !open_gate_toggled;
        OpenGateButton_ToggleChanged();
        SearchController.Instance.OpenGate_Toggled(open_gate_toggled);
    }

    private void OpenGateButton_ToggleChanged()
    {
        OpenGateButton.Modulate = OpenGateButton.Modulate.SetA(open_gate_toggled ? 1f : 0.5f);
    }
}
