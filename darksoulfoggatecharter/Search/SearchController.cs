using System;

public partial class SearchController : SingletonController
{
    public override string Directory => "Search";
    public static SearchController Instance => Singleton.Get<SearchController>();

    public event Action<string> OnSearchChanged;

    public void SetSearchTerm(string text)
    {
        OnSearchChanged?.Invoke(text);
    }
}
