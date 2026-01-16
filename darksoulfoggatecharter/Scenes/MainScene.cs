using Godot;

public partial class MainScene : Node3D
{
    [Export]
    public PackedScene MainView;

    public override void _Ready()
    {
        base._Ready();
        InitializeMainView();
    }

    private void InitializeMainView()
    {
        var view = MainView.Instantiate<MainView>();
        GetTree().Root.CallDeferred("add_child", view);
    }
}
