using System;

public class GateType : IEquatable<GateType>
{
    public static readonly GateType Traversable = new GateType("Traversable");
    public static readonly GateType Boss = new GateType("Boss");
    public static readonly GateType BossKilled = new GateType("BossKilled");
    public static readonly GateType Shortcut = new GateType("Shortcut"); // deprecated
    public static readonly GateType DoorShortcut = new GateType("DoorShortcut");
    public static readonly GateType OnewayShortcut = new GateType("One-wayShortcut");
    public static readonly GateType Warp = new GateType("Warp");
    public static readonly GateType PVP = new GateType("PVP");
    public static readonly GateType ItemObtained = new GateType("ItemObtained");
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

    public static bool operator ==(GateType left, GateType right) => left.Equals(right);
    public static bool operator !=(GateType left, GateType right) => !left.Equals(right);
    public static bool operator ==(GateType left, string right) => left.Equals(right);
    public static bool operator !=(GateType left, string right) => !left.Equals(right);
    public static bool operator ==(string left, GateType right) => right.Equals(left);
    public static bool operator !=(string left, GateType right) => !right.Equals(left);
}
