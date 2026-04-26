using Godot;

public partial class MeshNodeObject : NodeObject
{
    [Export]
    public Label3D Label;

    [Export]
    public MeshInstance3D Mesh;

    [Export]
    public MeshInstance3D Mesh_Select;

    private StandardMaterial3D material;

    public override void _Ready()
    {
        base._Ready();
        InitializeMesh();
    }

    private void InitializeMesh()
    {
        material = Mesh.GetActiveMaterial(0).Duplicate() as StandardMaterial3D;
        Mesh.SetSurfaceOverrideMaterial(0, material);
        Mesh_Select.Hide();
    }

    protected override void SetColor(Color color)
    {
        base.SetColor(color);
        material.AlbedoColor = color;
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);
        Mesh_Select.Visible = selected;
    }
}
