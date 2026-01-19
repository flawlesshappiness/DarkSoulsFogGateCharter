using Godot;
using System.Collections;
using System.Collections.Generic;

public static class Node3DExtensions
{
    public static void Disable(this Node3D node) => node.SetEnabled(false);
    public static void Enable(this Node3D node) => node.SetEnabled(true);

    public static void SetEnabled(this Node3D node, bool enabled)
    {
        node.Visible = enabled;
        node.ProcessMode = enabled ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
        //node.SetCollisionEnabled(enabled);
        node.SetSoundsEnabled(enabled);
    }

    public static void SetCollisionEnabled(this Node3D node, bool enabled)
    {
        var children = node.GetNodesInChildren<CollisionShape3D>();
        foreach (var child in children)
        {
            var child_enabled = enabled && child.IsVisibleInTree();
            child.Disabled = !child_enabled;
        }
    }

    public static void SetSoundsEnabled(this Node3D node, bool enabled)
    {
        var children = node.GetNodesInChildren<AudioStreamPlayer>();
        var children_2d = node.GetNodesInChildren<AudioStreamPlayer2D>();
        var children_3d = node.GetNodesInChildren<AudioStreamPlayer3D>();

        foreach (var child in children)
        {
            var parent = child.GetParent() as Node3D;
            if (parent == null) continue;

            if (parent.IsVisibleInTree())
            {
                if (!child.Playing && child.Autoplay)
                {
                    child.Play();
                }
            }
            else
            {
                child.Stop();
            }
        }

        foreach (var child in children_2d)
        {
            if (child.IsVisibleInTree())
            {
                if (!child.Playing && child.Autoplay)
                {
                    child.Play();
                }
            }
            else
            {
                child.Stop();
            }
        }

        foreach (var child in children_3d)
        {
            if (child.IsVisibleInTree())
            {
                if (!child.Playing && child.Autoplay)
                {
                    child.Play();
                }
            }
            else
            {
                child.Stop();
            }
        }
    }

    public static void ClearPositionAndRotation(this Node3D node)
    {
        node.Position = Vector3.Zero;
        node.Rotation = Vector3.Zero;
    }

    public static Coroutine Destroy(this Node3D node, float delay = 0.0f)
    {
        return node.StartCoroutine(Cr, "destroy");
        IEnumerator Cr()
        {
            if (delay > 0.0f)
            {
                yield return new WaitForSeconds(delay);
            }
            node.QueueFree();
        }
    }

    #region RAYCAST
    public static bool TryRaycast(this Node3D node, Vector3 start, Vector3 direction, float length, uint collision_mask, out RaycastResult3D result) => node.TryRaycast(start, start + direction.Normalized() * length, collision_mask, out result);
    public static bool TryRaycast(this Node3D node, Vector3 start, Vector3 end, uint collision_mask, out RaycastResult3D result)
    {
        var space = node.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(start, end, collision_mask);
        var intersection = space.IntersectRay(query);

        if (intersection.Keys.Count > 0)
        {
            intersection.TryGetValue("position", out var position);
            intersection.TryGetValue("normal", out var normal);
            intersection.TryGetValue("collider", out var collider);
            result = new RaycastResult3D
            {
                Position = position.AsVector3(),
                Normal = normal.AsVector3(),
                Collider = collider.AsGodotObject(),
            };
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public static bool TryRaycast(this Node2D node, Vector2 start, Vector2 direction, float length, uint collision_mask, out RaycastResult2D result) => node.TryRaycast(start, start + direction.Normalized() * length, collision_mask, out result);
    public static bool TryRaycast(this Node2D node, Vector2 start, Vector2 end, uint collision_mask, out RaycastResult2D result)
    {
        var space = node.GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(start, end, collision_mask);
        var intersection = space.IntersectRay(query);

        if (intersection.Keys.Count > 0)
        {
            intersection.TryGetValue("position", out var position);
            intersection.TryGetValue("normal", out var normal);
            intersection.TryGetValue("collider", out var collider);
            result = new RaycastResult2D
            {
                Position = position.AsVector2(),
                Normal = normal.AsVector2(),
                Collider = collider.AsGodotObject(),
            };
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public static IEnumerable<OverlapShapeResult3D> OverlapSphere(this Node3D node, Vector3 origin, float radius, uint collision_mask)
    {
        var shape = new SphereShape3D();
        shape.Radius = radius;
        return node.OverlapShape(shape, origin, collision_mask);
    }

    public static IEnumerable<OverlapShapeResult3D> OverlapShape(this Node3D node, Resource shape, Vector3 origin, uint collision_mask)
    {
        var space = node.GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D();
        query.Transform = query.Transform.Translated(origin);
        query.Shape = shape;
        query.CollisionMask = collision_mask;
        var intersections = space.IntersectShape(query);

        foreach (var intersection in intersections)
        {
            if (intersection.Keys.Count > 0)
            {
                intersection.TryGetValue("collider", out var collider);
                var result = new OverlapShapeResult3D
                {
                    Collider = collider.AsGodotObject(),
                };

                yield return result;
            }
        }
    }
    #endregion
}

public class RaycastResult3D
{
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    public GodotObject Collider { get; set; }
}

public class RaycastResult2D
{
    public Vector2 Position { get; set; }
    public Vector2 Normal { get; set; }
    public GodotObject Collider { get; set; }
}

public class OverlapShapeResult3D
{
    public GodotObject Collider { get; set; }
}