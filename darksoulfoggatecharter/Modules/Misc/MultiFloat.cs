using Godot.Collections;
using System.Linq;

public abstract class MultiFloat
{
    protected Dictionary<string, float> _values = new();

    public abstract float Value { get; }
    public float DefaultValue { get; private set; }

    public MultiFloat(float default_value = 0)
    {
        DefaultValue = default_value;
    }

    public bool HasValues => _values.Count > 0;

    public void Clear()
    {
        _values.Clear();
    }

    public void Set(string id, float value)
    {
        if (_values.ContainsKey(id))
        {
            _values[id] = value;
        }
        else
        {
            _values.Add(id, value);
        }
    }

    public void Remove(string id)
    {
        if (_values.ContainsKey(id))
        {
            _values.Remove(id);
        }
    }
}

public class MultiFloatMin : MultiFloat
{
    public override float Value => HasValues ? _values.Values.OrderByDescending(x => x).FirstOrDefault() : DefaultValue;
    public MultiFloatMin(float default_value = 0) : base(default_value) { }
}

public class MultiFloatMax : MultiFloat
{
    public override float Value => HasValues ? _values.Values.OrderBy(x => x).FirstOrDefault() : DefaultValue;
    public MultiFloatMax(float default_value = 0) : base(default_value) { }
}

public class MultiFloatSum : MultiFloat
{
    public override float Value => HasValues ? _values.Values.Sum() : DefaultValue;
    public MultiFloatSum(float default_value = 0) : base(default_value) { }
}