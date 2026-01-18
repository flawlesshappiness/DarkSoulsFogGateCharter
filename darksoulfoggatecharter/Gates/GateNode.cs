public class GateNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Connection { get; set; }
    public string Type { get; set; }
    public GateData Data { get; set; } = new();
}
