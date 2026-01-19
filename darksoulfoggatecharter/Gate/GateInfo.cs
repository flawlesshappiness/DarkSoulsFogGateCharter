using Godot;

[GlobalClass]
public partial class GateInfo : Resource
{
    private static GateInfo _instance;
    public static GateInfo Instance => _instance ?? (_instance = GD.Load<GateInfo>($"Gate/Resources/{nameof(GateInfo)}.tres"));

    [Export(PropertyHint.File)]
    public string GatesPath;
}
