using Godot;

public partial class DraggableCamera : Camera3D
{
    public static DraggableCamera Instance { get; private set; }
    public bool Dragging { get; private set; }
    public Vector2 MousePosition => GetViewport().GetMousePosition();
    public Vector3 MouseWorldPosition => ProjectPosition(MousePosition, GlobalPosition.Y);
    public Vector2 ViewportSize => GetViewport().GetVisibleRect().Size;
    public Vector3 ViewportWorldPosition => GlobalPosition.Set(y: 0);
    public float AspectRatio => CalculateAspectRatio();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        base._UnhandledInput(e);

        if (MainView.Instance?.HasActiveUI() ?? false) return;
        if (e is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Middle)
            {
                MousePressed(button.Pressed);
                GetViewport().SetInputAsHandled();
            }
            else if (button.ButtonIndex == MouseButton.WheelUp)
            {
                AdjustSize(-0.5f);
                GetViewport().SetInputAsHandled();
            }
            else if (button.ButtonIndex == MouseButton.WheelDown)
            {
                AdjustSize(0.5f);
                GetViewport().SetInputAsHandled();
            }
        }
        else if (e is InputEventMouseMotion motion && Dragging)
        {
            var rn = -motion.Relative / ViewportSize;
            var x = rn.X * Size * AspectRatio;
            var z = rn.Y * Size;
            var dir = new Vector3(x, 0, z);
            GlobalPosition += dir;
            GetViewport().SetInputAsHandled();
        }
    }

    private void MousePressed(bool pressed)
    {
        if (Dragging != pressed)
        {
            MousePressedChanged(pressed);
        }

        Dragging = pressed;
    }

    private void MousePressedChanged(bool pressed)
    {
        if (pressed)
        {
        }
        else
        {

        }
    }

    private float CalculateAspectRatio()
    {
        var size = ViewportSize;
        return size.X / size.Y;
    }

    private void AdjustSize(float value)
    {
        SetOrthographicSize(Size + value);
    }

    private void SetOrthographicSize(float value)
    {
        Size = Mathf.Clamp(value, 2f, 15f);
    }
}
