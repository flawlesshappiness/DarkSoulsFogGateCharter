using System.Collections.Generic;

public partial class SessionData : SaveData
{
    public List<GateData> Gates { get; set; } = new();
    public List<GroupData> Groups { get; set; } = new();
    public List<string> DisabledTypes { get; set; } = new();
}
