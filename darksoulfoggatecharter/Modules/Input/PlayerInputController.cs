using Godot;
using System;

public partial class PlayerInputController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/Input";
    public static PlayerInputController Instance => Singleton.Get<PlayerInputController>();

    public Action<bool> OnDeviceChanged;

    private bool is_gamepad;

    public override void _Input(InputEvent e)
    {
        base._Input(e);

        if (e is InputEventKey)
        {
            SetGamepad(false);
        }
        else if (e is InputEventMouse)
        {
            SetGamepad(false);
        }
        else if (e is InputEventJoypadButton)
        {
            SetGamepad(true);
        }
        else if (e is InputEventJoypadMotion)
        {
            SetGamepad(true);
        }
    }

    private void SetGamepad(bool gamepad)
    {
        if (is_gamepad == gamepad) return;
        is_gamepad = gamepad;
        OnDeviceChanged?.Invoke(is_gamepad);
    }
}
