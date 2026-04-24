using Godot;
using System;

public partial class DraggableCamera : Camera3D
{
    public static DraggableCamera Instance { get; private set; }
    public Vector2 MousePosition => GetViewport().GetMousePosition();
    public Vector3 MouseWorldPosition => ProjectPosition(MousePosition, GlobalPosition.Y);
    public Vector2 ViewportSize => GetViewport().GetVisibleRect().Size;
    public Vector3 ViewportWorldPosition => GlobalPosition.Set(y: 0);
    public float AspectRatio => CalculateAspectRatio();
    private Vector3 IntendedMoveDirection { get; set; }

    private const float MOVE_SPEED = 10.0f;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var fdelta = Convert.ToSingle(delta);

        if (IntendedMoveDirection.LengthSquared() > 0)
        {
            GlobalPosition += IntendedMoveDirection * MOVE_SPEED * fdelta;
        }
    }

    public void Drag(Vector2 relative)
    {
        var rn = -relative / ViewportSize;
        var x = rn.X * Size * AspectRatio;
        var z = rn.Y * Size;
        var dir = new Vector3(x, 0, z);
        GlobalPosition += dir;
    }

    public void Move(Vector2 dir)
    {
        IntendedMoveDirection = new Vector3(dir.X, 0, dir.Y);
    }

    private float CalculateAspectRatio()
    {
        var size = ViewportSize;
        return size.X / size.Y;
    }

    public void ZoomIn()
    {
        AdjustSize(-0.5f);
    }

    public void ZoomOut()
    {
        AdjustSize(0.5f);
    }

    private void AdjustSize(float value)
    {
        SetOrthographicSize(Size + value);
    }

    private void SetOrthographicSize(float value)
    {
        Size = Mathf.Clamp(value, 2f, 30f);
    }
}
