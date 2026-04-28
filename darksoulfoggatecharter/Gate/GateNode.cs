public class GateNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string Type { get; set; }
    public string Area { get; set; }
    public string DisplayName => _display_name ?? (_display_name = GetDisplayName());

    public bool HasId => !string.IsNullOrEmpty(Id);

    private string _display_name;

    private string GetDisplayName()
    {
        var name = Name;
        if (name.Contains('[') && name.Contains(']'))
        {
            var start = name.IndexOf('[');
            var end = name.IndexOf(']');
            var sub = name.Substring(start, end - start);
            name = name.Replace(sub, string.Empty);
        }

        name = name.Replace("[", string.Empty);
        name = name.Replace("]", string.Empty);

        return name;
    }
}
