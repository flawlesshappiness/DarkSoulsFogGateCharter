using Godot;
using System.Collections;

public partial class GroupNodeObject : NodeObject
{
    [Export]
    public AnimationPlayer Animation;

    public GateGroup Group { get; private set; }
    public override string NodeName => Group.Name;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
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
