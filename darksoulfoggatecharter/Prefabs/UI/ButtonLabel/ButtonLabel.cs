using Godot;

[Tool]
public partial class ButtonLabel : MarginContainer
{
    [Export]
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (Label != null)
            {
                Label.Text = value;
            }
        }
    }

    [Export]
    public Button Button;

    [Export]
    public Label Label;

    private bool IsEditor => Engine.IsEditorHint();

    private string _text;

    public override void _Ready()
    {
        base._Ready();

        if (!IsEditor)
        {
            Button.MouseEntered += Button_MouseEntered;
            Button.MouseExited += Button_MouseExited;
            Hide();
        }
    }

    private void Button_MouseEntered()
    {
        Show();
    }

    private void Button_MouseExited()
    {
        Hide();
    }
}
