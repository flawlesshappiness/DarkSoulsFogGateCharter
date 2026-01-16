using Godot;
using System;
using System.Collections.Generic;

public partial class ScrollPopupMenu : PopupMenu
{
    private bool has_mouse;

    private List<Action> actions = new();

    public override void _Ready()
    {
        base._Ready();
        MouseEntered += _MouseEntered;
        MouseExited += _MouseExited;
        IndexPressed += _IndexPressed;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        base._UnhandledInput(e);

        if (e is InputEventMouseButton button && has_mouse)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void _MouseEntered()
    {
        has_mouse = true;
    }

    private void _MouseExited()
    {
        has_mouse = false;
    }

    private void _IndexPressed(long l_index)
    {
        var i = (int)l_index;
        var action = actions[i];
        action?.Invoke();
    }

    public void ClearItems()
    {
        Clear();
        actions.Clear();
    }

    public void AddActionItem(string text, Action action)
    {
        AddItem(text);
        actions.Add(action);
    }
}
