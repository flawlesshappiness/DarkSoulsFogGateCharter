using Godot;

public partial class GateNodeObject : NodeObject
{
    [Export]
    public AnimationPlayer Animation;

    public GateNode Gate { get; private set; }

    public void SetGate(GateNode gate)
    {
        Gate = gate;
        Label.Text = gate.Name;
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
}
