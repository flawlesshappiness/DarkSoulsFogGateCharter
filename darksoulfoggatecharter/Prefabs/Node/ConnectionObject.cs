using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class ConnectionObject : Node3D
{
    [Export]
    public MeshInstance3D Mesh;

    [Export]
    public AnimationPlayer Animation;

    [Export]
    public StandardMaterial3D MaterialFull;

    [Export]
    public StandardMaterial3D MaterialDotted;

    public NodeObject ObjectA { get; private set; }
    public NodeObject ObjectB { get; private set; }
    public string NameA { get; private set; }
    public string NameB { get; private set; }
    public bool ConnectedToGroup { get; private set; }

    private BoxMesh box_mesh;
    private StandardMaterial3D material;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!IsInstanceValid(ObjectA)) return;
        if (!IsInstanceValid(ObjectB)) return;

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
        material.Uv1Scale = material.Uv1Scale.Set(y: dist * 30);
    }

    public void SetConnectedObjects(NodeObject A, NodeObject B)
    {
        ObjectA = A;
        ObjectB = B;
        NameA = A.NodeName;
        NameB = B.NodeName;
        ConnectedToGroup = IsConnectedToGroup();

        InitializeMesh();
        LoadColorPalette();
    }

    private void InitializeMesh()
    {
        box_mesh = Mesh.Mesh.Duplicate() as BoxMesh;
        Mesh.Mesh = box_mesh;
        material = ConnectedToGroup ? MaterialFull.Duplicate() as StandardMaterial3D : MaterialDotted.Duplicate() as StandardMaterial3D;
        Mesh.SetSurfaceOverrideMaterial(0, material);
    }

    private void SetColor(Color color)
    {
        material.AlbedoColor = color;
    }

    private void LoadColorPalette()
    {
        var area = GetArea();
        var info = ColorPaletteController.Instance.GetInfo(area);

        if (ConnectedToGroup)
        {
            SetColor(info.GetColor(1));
        }
        else
        {
            SetColor(info.GetColor(2));
        }
    }

    private string GetArea()
    {
        if (GateController.Instance.IsGroup(NameA))
        {
            return GateController.Instance.GetGroup(NameA).Area;
        }
        else
        {
            return GateController.Instance.GetGate(NameA).Area;
        }
    }

    private bool IsConnectedToGroup()
    {
        var names = new List<string> { NameA, NameB };
        return names.Any(GateController.Instance.IsGroup);
    }

    public void DestroyConnection()
    {
        this.StartCoroutine(Cr, "destroy");
        IEnumerator Cr()
        {
            yield return Animation.PlayAndWaitForAnimation("hide");
            QueueFree();
        }
    }
}
