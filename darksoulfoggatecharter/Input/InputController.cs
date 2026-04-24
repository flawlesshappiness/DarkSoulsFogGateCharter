using Godot;
using System;

public partial class InputController : SingletonController
{
    public static InputController Instance => Singleton.Get<InputController>();
    public override string Directory => "Input";
    private bool LeftMouseDown { get; set; }
    private bool RightMouseDown { get; set; }
    private bool MiddleMouseDown { get; set; }

    public Action OnShortcutQuicksave;

    public override void _UnhandledInput(InputEvent e)
    {
        base._UnhandledInput(e);

        if (e is InputEventMouse mouse)
        {
            Input_Mouse(mouse);
        }
        else if (e is InputEventKey key)
        {
            Input_Key(key);
        }
    }

    private void Input_Mouse(InputEventMouse e)
    {
        if (e is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left) // LMB
            {

                if (button.Pressed) // LMB pressed
                {
                    if (NodeObject.Hovered != null) // Press hovered node
                    {
                        NodeObject.Hovered.MousePressed(button.Pressed, button.CtrlPressed);
                    }
                    else // Press empty area
                    {
                        LeftMouseDown = true;
                    }
                }
                else // LMB released
                {
                    if (NodeObject.Handled != null) // Release currently handled node
                    {
                        NodeObject.Handled.MousePressed(button.Pressed, button.CtrlPressed);
                    }
                    else // Empty area
                    {
                        if (SelectionController.Instance.Dragging) // Release to select in area
                        {
                            SelectionController.Instance.StopSelectionArea(button.CtrlPressed);
                        }
                        else if (SelectionController.Instance.HasSelection) // Click to clear selection
                        {
                            SelectionController.Instance.ClearSelection();
                        }
                        else // Click empty area
                        {
                            MainView.Instance.EmptySpace_Clicked(DraggableCamera.Instance.MouseWorldPosition);
                        }

                        LeftMouseDown = false;
                    }
                }
            }
            else if (button.ButtonIndex == MouseButton.Middle) // MMB
            {
                MiddleMouseDown = button.Pressed;
            }
            else if (button.ButtonIndex == MouseButton.Right) // RMB
            {
                RightMouseDown = button.Pressed;
            }
            else if (button.ButtonIndex == MouseButton.WheelDown) // Wheel down
            {
                if (MainView.Instance.HasActiveUI()) return;
                DraggableCamera.Instance.ZoomOut();
            }
            else if (button.ButtonIndex == MouseButton.WheelUp) // Wheel up
            {
                if (MainView.Instance.HasActiveUI()) return;
                DraggableCamera.Instance.ZoomIn();
            }
        }
        else if (e is InputEventMouseMotion motion) // Mouse moved
        {
            if (NodeObject.Handled != null) // Is handling node
            {
                if (NodeObject.Handled.IsSelected) // Is handling a selection of nodes
                {
                    SelectionController.Instance.DragSelection();
                }
                else // Is handling an unselected node
                {
                    SelectionController.Instance.ClearSelection();
                    NodeObject.Handled.Drag();
                }
            }
            else if (LeftMouseDown) // Is dragging a selection area
            {
                SelectionController.Instance.StartSelectionArea();
            }

            if (!LeftMouseDown && (RightMouseDown || MiddleMouseDown)) // Drag camera
            {
                DraggableCamera.Instance.Drag(motion.Relative);
            }
        }
    }

    private void Input_Key(InputEventKey e)
    {
        if (e.Pressed) // Key pressed
        {

        }
        else // Key released
        {
            if (e.Keycode == Key.S && e.CtrlPressed) // Quicksave
            {
                OnShortcutQuicksave?.Invoke();
            }
        }

        if (!e.CtrlPressed) // Move camera
        {
            if (MainView.Instance.HasActiveUI()) return;
            DraggableCamera.Instance.Move(PlayerInput.GetMoveInput().Normalized());
        }
    }
}
