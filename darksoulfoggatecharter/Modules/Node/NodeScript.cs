using Godot;

public partial class NodeScript : Node
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

    public bool IsVisibleInTree()
    {
        var parent = GetParent();
        while (parent != null)
        {
            if (!Visible(parent))
            {
                return false;
            }

            parent = parent.GetParent();
        }

        return true;

        bool Visible(Node node)
        {
            if (node is Node3D n3)
            {
                return n3.IsVisibleInTree();
            }
            else if (node is Node2D n2)
            {
                return n2.IsVisibleInTree();
            }

            return true;
        }
    }
}
