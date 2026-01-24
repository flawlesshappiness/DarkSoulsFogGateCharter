using Godot;

public partial class SearchListItemButton : Button
{
    [Export]
    public Label NameLabel;

    [Export]
    public Label AreaLabel;

    public void SetGate(GateNode gate)
    {
        NameLabel.Text = gate.Name;
        AreaLabel.Text = gate.Area;
        AreaLabel.Modulate = ColorPaletteController.Instance.GetColor(gate.Area, 3);
        NameLabel.Modulate = ColorPaletteController.Instance.GetColor(gate.Area, 4);
    }
}
