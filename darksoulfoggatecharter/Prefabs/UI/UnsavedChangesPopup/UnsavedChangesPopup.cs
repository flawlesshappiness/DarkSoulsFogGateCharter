using Godot;
using System;

public partial class UnsavedChangesPopup : Control
{
    [Export]
    public Button YesButton;

    [Export]
    public Button NoButton;

    [Export]
    public Button CancelButton;

    public event Action YesPressed;
    public event Action NoPressed;
    public event Action CancelPressed;

    public override void _Ready()
    {
        base._Ready();
        YesButton.Pressed += Yes_Pressed;
        NoButton.Pressed += No_Pressed;
        CancelButton.Pressed += Cancel_Pressed;
    }

    private void Yes_Pressed()
    {
        Hide();
        YesPressed?.Invoke();
    }

    private void No_Pressed()
    {
        Hide();
        NoPressed?.Invoke();
    }

    private void Cancel_Pressed()
    {
        Hide();
        CancelPressed?.Invoke();
    }
}
