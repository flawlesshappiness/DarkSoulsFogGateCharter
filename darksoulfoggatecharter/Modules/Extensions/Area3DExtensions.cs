using Godot;
using System.Linq;

public static class Area3DExtensions
{
    public static Vector3 RandomPoint(this Area3D area)
    {
        var collision_shape = area.GetNodesInChildren<CollisionShape3D>().ToList().Random();
        var shape = collision_shape.Shape;
        var center = collision_shape.GlobalPosition;
        var rng = new RandomNumberGenerator();

        if (shape is BoxShape3D box && box != null)
        {
            var x = rng.RandfRange(-box.Size.X, box.Size.X);
            var y = rng.RandfRange(-box.Size.Y, box.Size.Y);
            var z = rng.RandfRange(-box.Size.Z, box.Size.Z);
            var position = new Vector3(x, y, z) * 0.5f;
            return center + position;
        }
        else if (shape is CylinderShape3D cylinder && cylinder != null)
        {
            var x = rng.RandfRange(-cylinder.Radius, cylinder.Radius);
            var y = rng.RandfRange(-cylinder.Height, cylinder.Height) * 0.5f;
            var z = rng.RandfRange(-cylinder.Radius, cylinder.Radius);
            var dir = new Vector3(x, 0, z).Normalized();
            var length = rng.RandfRange(-cylinder.Radius, cylinder.Radius);
            var position = dir * length + Vector3.Up * y;
            return center + position;
        }

        return center;
    }
}
