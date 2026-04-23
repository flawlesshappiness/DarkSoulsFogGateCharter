using Godot;

public partial class PlayerInput : Node
{
    public readonly static CustomInputAction Forward = new CustomInputAction("move_up");
    public readonly static CustomInputAction Back = new CustomInputAction("move_down");
    public readonly static CustomInputAction Left = new CustomInputAction("move_left");
    public readonly static CustomInputAction Right = new CustomInputAction("move_right");

    public static Vector2 GetMoveInput()
    {
        return Input.GetVector(
            Left.Name,
            Right.Name,
            Forward.Name,
            Back.Name);
    }
}
