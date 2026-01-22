using Godot;

public partial class MainScene : Scene
{
    public static MainScene Instance { get; private set; }

    [Export]
    public Node3D NodeParent;

    private MainView View => MainView.Instance;

    public override void _Ready()
    {
        base._Ready();

        Instance = this;

        CallDeferred(nameof(CreateStart));
    }

    private void CreateStart()
    {
        View.Show();
    }
}
