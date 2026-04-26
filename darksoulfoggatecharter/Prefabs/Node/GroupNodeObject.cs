using Godot;
using System.Collections;

public partial class GroupNodeObject : MeshNodeObject
{
    [Export]
    public AnimationPlayer Animation;

    [Export]
    public AnimationPlayer Animation_Search;

    [Export]
    public ColorPaletteInfo GreyedOutColorPalette;

    public GateGroup Group { get; private set; }
    public override string NodeName => Group.Name;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
        ImageMapMode_Changed(ImageMapController.Instance.ImageMapModeEnabled);
    }

    protected override void ImageMapMode_Changed(bool enabled)
    {
        base.ImageMapMode_Changed(enabled);
        Visible = !enabled;
    }

    protected override void Node_Created(NodeObject node)
    {
        if (node is GroupNodeObject group)
        {
            var relation = new NodeRelation
            {
                Node = node,
                MinDistance = 7f
            };

            Relations.Add(relation);
        }
    }

    protected override void SearchValid()
    {
        base.SearchValid();
        Animation_Search.Play("show");
        LoadColorPalette();
    }

    protected override void SearchInvalid()
    {
        base.SearchInvalid();
        Animation_Search.Play("hide");
        LoadColorPalette(GreyedOutColorPalette);
    }

    public void SetGroup(GateGroup group)
    {
        Group = group;
        Label.Text = group.Name;
        LoadColorPalette();
        InitializeOtherNodes();
    }

    public override void _MouseEnter()
    {
        if (IsDestroying) return;
        base._MouseEnter();
        Animation.Play("grow");
    }

    public override void _MouseExit()
    {
        if (IsDestroying) return;
        base._MouseExit();
        Animation.Play("shrink");
    }

    protected override void MousePressedChanged(bool pressed, bool ctrl)
    {
        if (IsDestroying) return;
        base.MousePressedChanged(pressed, ctrl);

        if (pressed)
        {
            Animation.Play("shrink");
        }
        else
        {
            Animation.Play("grow");
        }
    }

    private void LoadColorPalette()
    {
        var info = ColorPaletteController.Instance.GetInfo(Group.Area);
        LoadColorPalette(info);
    }

    private void LoadColorPalette(ColorPaletteInfo info)
    {
        SetColor(info.GetColor(1));
        Label.Modulate = info.GetColor(4);
    }

    public override void DestroyNode()
    {
        IsDestroying = true;
        this.StartCoroutine(Cr, "destroy");
        IEnumerator Cr()
        {
            Animation.Play("hide");
            yield return new WaitForSeconds(1);
            QueueFree();
        }
    }
}
