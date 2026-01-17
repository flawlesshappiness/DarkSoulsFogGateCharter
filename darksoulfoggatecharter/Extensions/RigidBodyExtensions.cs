using Godot;

public static class RigidBodyExtensions
{
    public static void LockPosition(this RigidBody3D rigidbody, bool x = false, bool y = false, bool z = false)
    {
        rigidbody.AxisLockLinearX = x;
        rigidbody.AxisLockLinearY = y;
        rigidbody.AxisLockLinearZ = z;
    }

    public static void UnlockPosition(this RigidBody3D rigidbody) => rigidbody.LockPosition();

    public static void LockRotation(this RigidBody3D rigidbody, bool x = false, bool y = false, bool z = false)
    {
        rigidbody.AxisLockAngularX = x;
        rigidbody.AxisLockAngularY = y;
        rigidbody.AxisLockAngularZ = z;
    }

    public static void UnlockRotation(this RigidBody3D rigidbody) => rigidbody.LockRotation();
}
