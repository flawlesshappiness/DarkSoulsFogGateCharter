using Godot;
using System.Collections;

public partial class GateNodeObject : MeshNodeObject
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
    public override string NodeArea => Gate.Area;
    public GateNode Gate { get; private set; }

    private float spawn_time;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play("show");
        spawn_time = GameTime.Time + 0.5f;
    }

    public override void Initialize(string name)
    {
        Gate = GateController.Instance.GetGate(name);
        Label.Text = Gate.DisplayName;
        UpdateIcon(Gate.Type);
        base.Initialize(name);
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

        // ImageMapMode
        ImageMapTarget = ImageMapController.Instance.GetMapMarker(Gate.Name);
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

    protected override void LoadColorPalette(ColorPaletteInfo info)
    {
        SetColor(info.GetColor(2));
        Label.Modulate = info.GetColor(4);

        IconObjective.Modulate = info.GetColor(1);
        IconShortcut.Modulate = info.GetColor(1);
        IconShortcutOneway.Modulate = info.GetColor(1);
        IconLocked.Modulate = info.GetColor(1);
    }

    protected override void LoadGreyedOutColorPalette()
    {
        base.LoadGreyedOutColorPalette();
        LoadColorPalette(GreyedOutColorPalette);
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
