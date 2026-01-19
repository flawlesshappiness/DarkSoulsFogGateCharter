using Godot;

public abstract partial class ResourceController<C, R> : SingletonController
    where C : ResourceCollection<R>
    where R : Resource
{
    private C _collection;
    public C Collection => GetCollection();
    protected virtual string CustomResourceCollectionDirectory { get; }
    protected C GetCollection() => _collection ?? (_collection = LoadCollection(GetResourceCollectionPath()));
    private C LoadCollection(string path) => ResourceCollection<R>.Load<C>(path);

    private string GetResourceCollectionPath()
    {
        var collection_filename = $"{typeof(C).Name}.tres";
        var default_path = $"{Directory}/Resources/{collection_filename}";
        var custom_path = $"{CustomResourceCollectionDirectory}/Resources/{collection_filename}";
        return string.IsNullOrEmpty(CustomResourceCollectionDirectory) ? default_path : custom_path;
    }
}
