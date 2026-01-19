using System.Collections.Generic;

public class AreaNode
{
    public string Name { get; set; }
    public List<GateNode> Gates { get; set; } = new();
}
