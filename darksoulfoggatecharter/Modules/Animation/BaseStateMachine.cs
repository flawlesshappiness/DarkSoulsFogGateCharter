using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlawLizArt.Animation.StateMachine;

public abstract partial class BaseStateMachine : Node
{
    public bool Started { get; private set; }
    public StateNode Current { get; private set; }
    public StateNode Any { get; private set; } = new(nameof(Any));

    public List<StateNode> Nodes => _nodes.ToList();
    public List<Connection> Connections => _connections.ToList();
    public List<Parameter> Parameters => _parameters.ToList();

    private List<StateNode> _nodes = new();
    private List<Connection> _connections = new();
    private List<Parameter> _parameters = new();

    public void Start(StateNode node)
    {
        SetCurrentState(node);
        Started = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!Started) return;
        if (TryProcessAnyState(false)) return;
        if (TryProcessCurrentState(false)) return;
    }

    protected bool TryProcessCurrentState(bool include_conditionless) => TryProcessNode(Current, include_conditionless);
    protected bool TryProcessAnyState(bool include_conditionless) => TryProcessNode(Any, include_conditionless);
    private bool TryProcessNode(StateNode node, bool include_conditionless)
    {
        var connection = node.ValidConnection(include_conditionless);
        if (connection == null) return false;

        UseConnection(connection);
        return true;
    }

    private void UseConnection(Connection connection)
    {
        connection.Used();
        SetCurrentState(connection.End);
    }

    public virtual void SetCurrentState(StateNode node)
    {
        if (Current != null)
        {
            Current.OnExit?.Invoke();
        }

        Current = node;

        if (Current != null)
        {
            Current.OnEnter?.Invoke();
        }
    }

    public StateNode CreateNode(string name)
    {
        var node = new StateNode(name);
        _nodes.Add(node);
        return node;
    }

    public FloatParameter CreateParameter(string name, float value) => CreateParameter(new FloatParameter(name, value)) as FloatParameter;
    public IntParameter CreateParameter(string name, int value) => CreateParameter(new IntParameter(name, value)) as IntParameter;
    public BoolParameter CreateParameter(string name, bool value) => CreateParameter(new BoolParameter(name, value)) as BoolParameter;
    public TriggerParameter CreateParameter(string name) => CreateParameter(new TriggerParameter(name)) as TriggerParameter;

    private Parameter CreateParameter(Parameter parameter)
    {
        _parameters.Add(parameter);
        return parameter;
    }

    public void Connect(StateNode start, StateNode end, params Condition[] conditions)
    {
        if (start == null)
        {
            Debug.LogError("Start node is null");
        }

        if (end == null)
        {
            Debug.LogError("End node is null");
        }

        var connection = new Connection(start, end, conditions);
        Connect(connection);
    }

    public void ConnectFromAnyState(StateNode end, params Condition[] conditions)
    {
        if (end == null)
        {
            Debug.LogError("End node is null");
        }

        if (conditions == null || conditions.Length == 0)
        {
            Debug.LogError("Connection from any state must have at least 1 condition");
        }

        var connection = new Connection(Any, end, conditions);
        Connect(connection);
    }

    private void Connect(Connection connection)
    {
        _connections.Add(connection);

        if (connection.Start != null)
        {
            connection.Start.AddConnection(connection);
        }
    }

    public Parameter<V> GetParameter<V>(string name)
        where V : IComparable
    {
        return Parameters.FirstOrDefault(x => x.Name == name) as Parameter<V>;
    }
}

public class StateNode
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public List<Connection> Connections => _connections.ToList();
    private List<Connection> _connections = new();

    public Action OnEnter, OnExit;

    public StateNode(string name)
    {
        Name = name;
    }

    public Connection ValidConnection(bool include_conditionless) => _connections.FirstOrDefault(x => x.Validate(include_conditionless));
    public void AddConnection(Connection connection) => _connections.Add(connection);
}

public class Connection
{
    public StateNode Start { get; private set; }
    public StateNode End { get; private set; }
    public List<Condition> Conditions => _conditions.ToList();
    public bool HasConditions => _conditions.Count > 0;

    private List<Condition> _conditions = new();

    public Connection(StateNode start, StateNode end, params Condition[] conditions)
    {
        Start = start;
        End = end;
        _conditions.AddRange(conditions);
    }

    public bool Validate(bool include_conditionless) => (include_conditionless && !HasConditions) || (HasConditions && _conditions.All(x => x.Validate()));
    public void Used() => _conditions.ForEach(x => x.Used());

    // Transition
}

public abstract class Condition
{
    public abstract bool Validate();
    public virtual void Initialize() { }
    public virtual void Used() { }

    public static Condition<float> CreateFloat(FloatParameter parameter, ComparisonType comparison_type, float value) =>
        new Condition<float>(parameter, comparison_type, value);

    public static Condition<int> CreateInt(IntParameter parameter, ComparisonType comparison_type, int value) =>
        new Condition<int>(parameter, comparison_type, value);
}

public class Condition<V> : Condition
    where V : IComparable
{
    public Parameter<V> Parameter { get; private set; }
    public ComparisonType ComparisonType { get; private set; }
    public V Value { get; private set; }

    public Condition(Parameter<V> parameter, ComparisonType comparison_type, V value)
    {
        Parameter = parameter;
        ComparisonType = comparison_type;
        Value = value;
    }

    public override bool Validate() => ComparisonType switch
    {
        ComparisonType.Equal => Parameter.Value.CompareTo(Value) == 0,
        ComparisonType.NotEqual => Parameter.Value.CompareTo(Value) != 0,
        ComparisonType.LessThan => Parameter.Value.CompareTo(Value) < 0,
        ComparisonType.GreaterThan => Parameter.Value.CompareTo(Value) > 0,
        ComparisonType.LessOrEqual => Parameter.Value.CompareTo(Value) <= 0,
        ComparisonType.GreaterOrEqual => Parameter.Value.CompareTo(Value) >= 0,
        _ => false
    };

    public override void Used()
    {
        base.Used();
        Parameter.Used();
    }
}

public abstract class Parameter
{
    public string Name { get; private set; }

    public Parameter(string name)
    {
        Name = name;
    }

    public virtual void Used() { }
}

public abstract class Parameter<V> : Parameter
    where V : IComparable
{
    public V Value { get; private set; }

    public Action<V> OnValueChanged;

    public Parameter(string name, V value) : base(name)
    {
        Value = value;
    }

    public void Set(V value)
    {
        Value = value;
        OnValueChanged?.Invoke(Value);
    }

    public Condition<V> When(ComparisonType comparison_type, V value) => new Condition<V>(this, comparison_type, value);
}

public class FloatParameter : Parameter<float>
{
    public FloatParameter(string name, float value) : base(name, value) { }

    public void Add(float value)
    {
        Set(Value + value);
    }

    public Condition<float> WhenNegative() => new Condition<float>(this, ComparisonType.LessThan, 0f);
    public Condition<float> WhenPositive() => new Condition<float>(this, ComparisonType.GreaterThan, 0f);
}

public class IntParameter : Parameter<int>
{
    public IntParameter(string name, int value) : base(name, value) { }

    public void Add(int value)
    {
        Set(Value + value);
    }

    public Condition<int> WhenNegative() => new Condition<int>(this, ComparisonType.LessThan, 0);
    public Condition<int> WhenPositive() => new Condition<int>(this, ComparisonType.GreaterThan, 0);
    public Condition<int> WhenZero() => new Condition<int>(this, ComparisonType.Equal, 0);
}

public class BoolParameter : Parameter<bool>
{
    public BoolParameter(string name, bool value) : base(name, value) { }

    public void Toggle()
    {
        Set(!Value);
    }

    public Condition<bool> WhenTrue() => new Condition<bool>(this, ComparisonType.Equal, true);
    public Condition<bool> WhenFalse() => new Condition<bool>(this, ComparisonType.Equal, false);
}

public class TriggerParameter : Parameter<bool>
{
    public TriggerParameter(string name) : base(name, false)
    {
    }

    public void Trigger()
    {
        Set(true);
    }

    public void Untrigger()
    {
        Set(false);
    }

    public override void Used()
    {
        base.Used();
        Untrigger();
    }

    public Condition<bool> WhenTriggered() => new Condition<bool>(this, ComparisonType.Equal, true);
}

public class RandomParameter : Parameter<float>
{
    private RandomNumberGenerator rng = new();
    public RandomParameter(string name, float chance) : base(name, chance) { }
    public Condition<float> WhenTriggered() => new Condition<float>(this, ComparisonType.LessThan, rng.Randf());
}

public enum ComparisonType
{
    Equal, NotEqual,
    LessThan, GreaterThan,
    GreaterOrEqual, LessOrEqual,
}