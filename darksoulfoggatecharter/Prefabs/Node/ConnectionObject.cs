using Godot;

public partial class ConnectionObject : Node3D
{
    [Export]
    public MeshInstance3D Mesh;

    public Node3D ConnectedObjectA { get; private set; }
    public Node3D ConnectedObjectB { get; private set; }

    private BoxMesh box_mesh;

    public override void _Ready()
    {
        base._Ready();
        box_mesh = Mesh.Mesh.Duplicate() as BoxMesh;
        Mesh.Mesh = box_mesh;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (ConnectedObjectA == null) return;
        if (ConnectedObjectB == null) return;

        var pos_A = ConnectedObjectA.Position.Set(y: 0);
        var pos_B = ConnectedObjectB.Position.Set(y: 0);
        var dir = pos_B - pos_A;
        var dist = dir.Length();
        var angle = Vector3.Forward.SignedAngleTo(dir, Vector3.Up);
        var position = pos_A.Lerp(pos_B, 0.5f);

        GlobalPosition = position;
        GlobalRotation = new Vector3(0, angle, 0);

        box_mesh.Size = box_mesh.Size.Set(z: dist);
    }

    public void SetConnectedObjects(Node3D A, Node3D B)
    {
        ConnectedObjectA = A;
        ConnectedObjectB = B;
    }
}
