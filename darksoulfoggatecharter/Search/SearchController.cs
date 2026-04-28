using System;

public partial class SearchController : SingletonController
{
    public override string Directory => "Search";
    public static SearchController Instance => Singleton.Get<SearchController>();

    public string CurrentSearchTerm { get; private set; }
    public bool OpenGateToggled { get; private set; }

    public event Action<string> OnSearchChanged;
    public event Action<bool> OnOpenGateToggled;

    public void SetSearchTerm(string text)
    {
        CurrentSearchTerm = text;
        OnSearchChanged?.Invoke(text);
    }

    public void OpenGate_Toggled(bool toggled)
    {
        OpenGateToggled = toggled;
        OnOpenGateToggled?.Invoke(toggled);
    }
}
