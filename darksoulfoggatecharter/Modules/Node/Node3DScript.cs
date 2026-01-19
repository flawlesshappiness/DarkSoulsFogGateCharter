using Godot;

public partial class Node3DScript : Node3D
{
    private bool _initialized = false;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }
    }

    protected virtual void Initialize()
    {
    }
}
