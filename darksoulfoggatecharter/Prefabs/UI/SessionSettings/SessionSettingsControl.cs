using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SessionSettingsControl : MarginContainer
{
    [Export]
    public CheckBox TraversableCheck;

    [Export]
    public CheckBox GoldenCheck;

    [Export]
    public CheckBox PvpCheck;

    [Export]
    public CheckBox BossCheck;

    [Export]
    public CheckBox WarpCheck;

    [Export]
    public CheckBox ObjectiveCheck;

    [Export]
    public Button ConfirmButton;

    [Export]
    public Button CancelButton;

    public override void _Ready()
    {
        base._Ready();
        CancelButton.Pressed += Cancel_Pressed;
    }

    public SessionData CreateData()
    {
        var data = new SessionData();
        data.DisabledTypes = GetDisabledTypes();
        return data;
    }

    private List<string> GetDisabledTypes()
    {
        var dic = new Dictionary<CheckBox, string>
        {
            { TraversableCheck, GateType.Traversable.ToString() },
            { GoldenCheck, GateType.Golden.ToString() },
            { PvpCheck, GateType.PVP.ToString() },
            { BossCheck, GateType.Boss.ToString() },
            { WarpCheck, GateType.Warp.ToString() },
            { ObjectiveCheck, GateType.Objective.ToString() },
        };

        var list = dic
            .Where(x => !x.Key.ButtonPressed)
            .Select(x => x.Value).ToList();

        return list;
    }

    private void Cancel_Pressed()
    {
        Hide();
    }
}
