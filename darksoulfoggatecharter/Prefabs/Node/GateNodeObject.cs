using Godot;
using System.Collections;

public partial class GateNodeObject : NodeObject
{
    [Export]
    public AnimationPlayer Animation;

    [Export]
    public AnimationPlayer Animation_Search;

    [Export]
    public Sprite3D IconObjective;

    [Export]
    public Sprite3D IconShortcut;

    [Export]
    public Sprite3D IconShortcutOneway;

    [Export]
    public Sprite3D IconLocked;

    [Export]
    public ColorPaletteInfo GreyedOutColorPalette;

    public override string NodeName => Gate.Name;
    public GateNode Gate { get; private set; }

    private float spawn_time;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
        spawn_time = GameTime.Time + 0.5f;
    }

    protected override void Node_Created(NodeObject node)
    {
        base.Node_Created(node);

        if (node is GroupNodeObject group)
        {
            var relation = GetOrCreateRelation(node);
            relation.MinDistance = 1.7f;

            if (Gate.Location == group.NodeName)
            {
                relation.MaxDistance = 2.2f;
            }
        }
        else if (node is GateNodeObject gate)
        {
            var relation = GetOrCreateRelation(node);
            relation.MinDistance = 1.5f;
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

    public void SetGate(GateNode gate)
    {
        Gate = gate;
        Label.Text = gate.Name;
        LoadColorPalette();
        UpdateIcon(gate.Type);
        InitializeOtherNodes();
    }

    public override void _MouseEnter()
    {
        if (IsDestroying) return;
        base._MouseEnter();
        if (IsHandled) return;
        Animation.Play("grow");
    }

    public override void _MouseExit()
    {
        if (IsDestroying) return;
        base._MouseExit();
        if (IsHandled) return;
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
        LoadColorPalette(info);
    }

    private void LoadColorPalette(ColorPaletteInfo info)
    {
        SetColor(info.GetColor(2));
        Label.Modulate = info.GetColor(4);

        IconObjective.Modulate = info.GetColor(1);
        IconShortcut.Modulate = info.GetColor(1);
        IconShortcutOneway.Modulate = info.GetColor(1);
        IconLocked.Modulate = info.GetColor(1);
    }

    public override void AddConnection(string id, NodeObject node)
    {
        base.AddConnection(id, node);

        if (GameTime.Time > spawn_time)
        {
            Animation.Play("bounce");
        }

        if (node is GateNodeObject gate)
        {
            Node_Created(node);
        }
    }

    public override void DestroyNode()
    {
        IsDestroying = true;
        this.StartCoroutine(Cr, "destroy");
        IEnumerator Cr()
        {
            Animation.Play("hide");
            yield return new WaitForSeconds(1f);
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
