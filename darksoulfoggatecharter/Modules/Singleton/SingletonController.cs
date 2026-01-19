using System;

public abstract partial class SingletonController : NodeScript, IComparable<SingletonController>
{
    public abstract string Directory { get; }
    public SingletonController Create() => Singleton.GetOrCreate($"{Directory}/{GetType().Name}", GetType()) as SingletonController;

    public static void CreateAll()
    {
        var singletons = ReflectiveEnumerator.GetEnumerableOfType<SingletonController>();
        foreach (var singleton in singletons)
        {
            singleton.Create();
        }
    }

    public int CompareTo(SingletonController other)
    {
        return GetType().Name.CompareTo(other.GetType().Name);
    }

    protected override void Initialize()
    {
        base.Initialize();
        Debug.TraceMethod(GetType());
    }
}
