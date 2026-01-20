using Godot;

public partial class GateNodeObject : NodeObject
{
    [Export]
    public AnimationPlayer Animation;

    public override string NodeName => Gate.Name;

    public GateNode Gate { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
    }

    public void SetGate(GateNode gate)
    {
        Gate = gate;
        Label.Text = gate.Name;
        LoadColorPalette();
    }

    public override void _MouseEnter()
    {
        base._MouseEnter();
        if (IsHandled) return;
        Animation.Play("grow");
    }

    public override void _MouseExit()
    {
        base._MouseExit();
        if (IsHandled) return;
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

    public GateData ToData()
    {
        return new GateData
        {
            Name = Gate.Name,
            X = GlobalPosition.X,
            Y = GlobalPosition.Y,
            Z = GlobalPosition.Z,
        };
    }

    private void LoadColorPalette()
    {
        var info = ColorPaletteController.Instance.GetInfo(Gate.Area);
        SetColor(info.GetColor(2));
        Label.Modulate = info.GetColor(4);
        SetGlow(info.GetColor(0));
    }
}
