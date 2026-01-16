using Godot;

public partial class AreaGraphNode : GraphNode
{
    [Export]
    public Label GateLabelTemplate;

    public override void _Ready()
    {
        base._Ready();
        GateLabelTemplate.Hide();
    }

    public void SetArea(string area)
    {
        Title = area;
    }

    public void AddGate(string gate)
    {
        var label = GateLabelTemplate.Duplicate() as Label;
        GateLabelTemplate.GetParent().AddChild(label);
        label.Text = gate;
        label.Show();

        var i = label.GetIndex() - 1;
        SetSlotEnabledLeft(i, true);
        SetSlotEnabledRight(i, true);
    }
}
