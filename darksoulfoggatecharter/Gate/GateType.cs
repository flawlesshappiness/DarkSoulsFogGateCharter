using System;

public class GateType : IEquatable<GateType>
{
    public static readonly GateType Traversable = new GateType("Traversable");
    public static readonly GateType Boss = new GateType("Boss");
    public static readonly GateType DoorShortcut = new GateType("DoorShortcut");
    public static readonly GateType OnewayShortcut = new GateType("OnewayShortcut");
    public static readonly GateType ShortcutExit = new GateType("ShortcutExit");
    public static readonly GateType Warp = new GateType("Warp");
    public static readonly GateType PVP = new GateType("PVP");
    public static readonly GateType Objective = new GateType("Objective");
    public static readonly GateType Area = new GateType("Area");
    public static readonly GateType Golden = new GateType("Golden");


    private string id;

    public GateType(string id)
    {
        this.id = id;
    }

    public bool Equals(GateType other)
    {
        return id == other?.id;
    }

    public bool Equals(string other)
    {
        if (other == null) return false;
        return id == other;
    }

    public override string ToString()
    {
        return id;
    }

    public static bool operator ==(GateType left, GateType right) => left.Equals(right);
    public static bool operator !=(GateType left, GateType right) => !left.Equals(right);
    public static bool operator ==(GateType left, string right) => left.Equals(right);
    public static bool operator !=(GateType left, string right) => !left.Equals(right);
    public static bool operator ==(string left, GateType right) => right.Equals(left);
    public static bool operator !=(string left, GateType right) => !right.Equals(left);
}
