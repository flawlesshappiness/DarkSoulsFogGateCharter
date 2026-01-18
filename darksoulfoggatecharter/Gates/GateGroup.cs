using System.Collections.Generic;

public class GateGroup
{
    public string Name { get; set; }
    public Dictionary<string, GateNode> Gates { get; set; } = new();
}
