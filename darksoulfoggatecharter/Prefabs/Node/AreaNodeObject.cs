public partial class AreaNodeObject : NodeObject
{
    public AreaNode Area { get; private set; }

    public void SetArea(AreaNode area)
    {
        Area = area;
        Label.Text = area.Name;
    }
}
