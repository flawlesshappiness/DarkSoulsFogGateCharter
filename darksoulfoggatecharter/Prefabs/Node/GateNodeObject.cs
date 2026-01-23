using Godot;
using System.Collections;

public partial class GateNodeObject : NodeObject
{
    [Export]
    public AnimationPlayer Animation;

    [Export]
    public Sprite3D IconObjective;

    [Export]
    public Sprite3D IconShortcut;

    [Export]
    public Sprite3D IconShortcutOneway;

    [Export]
    public Sprite3D IconLocked;

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
        UpdateIcon(gate.Type);
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

        IconObjective.Modulate = info.GetColor(1);
        IconShortcut.Modulate = info.GetColor(1);
        IconLocked.Modulate = info.GetColor(1);
    }

    public override void AddConnection(string id, NodeObject node)
    {
        base.AddConnection(id, node);
        Animation.Play("bounce");
    }

    public override void DestroyNode()
    {
        this.StartCoroutine(Cr, "destroy");
        IEnumerator Cr()
        {
            yield return Animation.PlayAndWaitForAnimation("hide");
            QueueFree();
        }
    }

    private void ClearIcons()
    {
        IconObjective.Hide();
        IconShortcut.Hide();
        IconShortcutOneway.Hide();
        IconLocked.Hide();
    }

    public void UpdateIcon(string type)
    {
        ClearIcons();

        switch (type)
        {
            case GateType.Objective:
                IconObjective.Show();
                break;
            case GateType.DoorShortcut:
                IconShortcut.Show();
                break;
            case GateType.OnewayShortcut:
                IconShortcutOneway.Show();
                break;
            case GateType.LockedDoor:
                IconLocked.Show();
                break;
            default: break;
        }

        var icon = type switch
        {
            GateType.DoorShortcut => IconShortcut,
            _ => null
        };

        if (icon == null) return;
        icon.Show();
    }
}
