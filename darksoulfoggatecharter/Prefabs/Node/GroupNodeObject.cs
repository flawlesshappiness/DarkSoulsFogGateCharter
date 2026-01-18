using Godot;

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

    public void SetGroup(GateGroup group)
    {
        Group = group;
        Label.Text = group.Name;
    }

    public override void _MouseEnter()
    {
        base._MouseEnter();
        Animation.Play("grow");
    }

    public override void _MouseExit()
    {
        base._MouseExit();
        Animation.Play("shrink");
    }

    protected override void MousePressedChanged(bool pressed)
    {
        base.MousePressedChanged(pressed);

        if (pressed)
        {
            Animation.Play("shrink");
        }
        else
        {
            Animation.Play("grow");
        }
    }
}
