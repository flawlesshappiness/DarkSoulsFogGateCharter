using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract partial class View : ControlScript, IComparable<View>
{
    [Export]
    public int ChildOrder;

    protected virtual bool IgnoreCreate => false;
    public virtual string Directory => $"{Paths.ViewDirectory}/{GetType().Name}";
    private View Create()
    {
        if (IgnoreCreate)
        {
            return null;
        }
        else
        {
            return Singleton.LoadScene($"{Directory}/{GetType().Name}", GetType()) as View;
        }
    }

    public static void CreateAll()
    {
        var types = ReflectiveEnumerator.GetEnumerableOfType<View>();
        var views = new List<View>();

        // Create views
        foreach (var type in types)
        {
            var v = type.Create();
            if (v != null)
            {
                views.Add(v);
            }
        }

        // Order views
        var ordered_views = views.OrderBy(x => x.ChildOrder).ToList();
        var child_count = Scene.Root.GetChildCount();
        var idx_start = child_count - views.Count - 1;

        for (int i = 0; i < ordered_views.Count; i++)
        {
            var idx = idx_start + i;
            var view = ordered_views[i];
            Scene.Root.MoveChild(view, idx);
        }
    }

    public override void _Ready()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;
        base._Ready();
    }

    public static T Get<T>() where T : View =>
        Singleton.TryGet<T>(out var view) ? view : null;

    public static void Show<T>() where T : View =>
        Get<T>().Show();

    public int CompareTo(View other)
    {
        return GetType().Name.CompareTo(other.GetType().Name);
    }
}
