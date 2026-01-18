using Godot;

public partial class ConnectionObject : Node3D
{
    [Export]
    public MeshInstance3D Mesh;

    [Export]
    public AnimationPlayer Animation;

    public NodeObject ObjectA { get; private set; }
    public NodeObject ObjectB { get; private set; }
    public string NameA { get; private set; }
    public string NameB { get; private set; }

    private BoxMesh box_mesh;

    public override void _Ready()
    {
        base._Ready();
        box_mesh = Mesh.Mesh.Duplicate() as BoxMesh;
        Mesh.Mesh = box_mesh;

        Animation.Play("show");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (ObjectA == null) return;
        if (ObjectB == null) return;

        var pos_A = ObjectA.Position.Set(y: 0);
        var pos_B = ObjectB.Position.Set(y: 0);
        var dir = pos_B - pos_A;
        var length = dir.Length();
        var dist = length;
        var angle = Vector3.Forward.SignedAngleTo(dir, Vector3.Up);
        var position = pos_A.Lerp(pos_B, 0.5f);

        GlobalPosition = position;
        GlobalRotation = new Vector3(0, angle, 0);

        box_mesh.Size = box_mesh.Size.Set(z: dist);
    }

    public void SetConnectedObjects(NodeObject A, NodeObject B)
    {
        ObjectA = A;
        ObjectB = B;
        NameA = A.NodeName;
        NameB = B.NodeName;
    }
}
