using Godot;

public static class RandomNumberGeneratorExtensions
{
    public static Vector2 RandCircDirection(this RandomNumberGenerator rng)
    {
        return Vector2.Up.Rotated(rng.RandfRange(0, 360f));
    }
}
