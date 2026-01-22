using Godot;

public partial class MainScene : Scene
{
    public static MainScene Instance { get; private set; }

    [Export]
    public Node3D NodeParent;

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        CallDeferred(nameof(CreateStart));
    }

    private void CreateStart()
    {
        MainView.Instance.Show();
    }
}
