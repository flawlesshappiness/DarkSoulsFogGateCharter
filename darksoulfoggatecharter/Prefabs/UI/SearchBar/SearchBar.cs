using Godot;

public partial class SearchBar : MarginContainer
{
    [Export]
    public TextEdit TextEdit;

    [Export]
    public Button SearchButton;

    public override void _Ready()
    {
        base._Ready();
        TextEdit.TextChanged += TextEdit_TextChanged;
        InputController.Instance.OnNodeClicked += _ => ClearFocus();
        InputController.Instance.OnEmptySpaceClicked += ClearFocus;
        InputController.Instance.OnDrag += ClearFocus;
        InputController.Instance.OnShortcutSearch += Shortcut_Search;
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
}
