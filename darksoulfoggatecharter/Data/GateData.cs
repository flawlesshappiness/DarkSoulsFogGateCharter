using System.Collections.Generic;

public class GateData
{
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public List<string> Connections { get; set; } = new();
}
