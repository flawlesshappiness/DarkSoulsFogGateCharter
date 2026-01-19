using Godot;

public static class ControlExtensions
{
    public static void SetMouseFilterRec(this Control control, Control.MouseFilterEnum mouseFilter)
    {
        control
            .GetNodesInChildren<Control>()
            .ForEach(x => x.MouseFilter = mouseFilter);
    }
}
