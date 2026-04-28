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
    public override string NodeArea => Group.Area;
    public override bool IsGroup => true;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
        ImageMapMode_Changed(ImageMapController.Instance.ImageMapModeEnabled);
    }

    public override void Initialize(string name)
    {
        Group = GateController.Instance.GetGroup(name);
        Label.Text = name;
        base.Initialize(name);
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

    protected override void DisplayValid(bool valid)
    {
        base.DisplayValid(valid);
        if (valid)
        {
            Animation_Search.Play("show");
            LoadColorPalette();
        }
        else
        {
            Animation_Search.Play("hide");
            LoadColorPalette(GreyedOutColorPalette);
        }
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

    protected override void LoadColorPalette(ColorPaletteInfo info)
    {
        SetColor(info.GetColor(1));
        Label.Modulate = info.GetColor(4);
    }

    protected override void LoadGreyedOutColorPalette()
    {
        base.LoadGreyedOutColorPalette();
        LoadColorPalette(GreyedOutColorPalette);
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
