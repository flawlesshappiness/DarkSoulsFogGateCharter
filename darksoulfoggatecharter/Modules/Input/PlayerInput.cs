using Godot;

public partial class PlayerInput : Node
{

}

public class CustomInputAction
{
    public string Name { get; set; }
    public bool Pressed => Input.IsActionJustPressed(Name);
    public bool Held => Input.IsActionPressed(Name);
    public bool Released => Input.IsActionJustReleased(Name);

    public CustomInputAction(string name)
    {
        Name = name;
    }
}