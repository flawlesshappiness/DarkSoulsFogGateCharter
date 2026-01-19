using Godot;
using System;
using System.Collections;

public class Coroutine : CustomYieldInstruction
{
    public Guid Id { get; set; }
    public string StringId { get; set; }
    public IEnumerator Enumerator { get; set; }
    public bool HasCompleted { get; set; }
    public bool HasEnded { get; set; }
    public Node ConnectedNode { get; private set; }
    public bool HasConnectedNode { get; private set; }
    public bool RunWhilePaused { get; private set; }
    public override bool KeepWaiting => !HasEnded;

    public Coroutine(IEnumerator enumerator)
    {
        Enumerator = enumerator;
    }

    public static Coroutine Start(IEnumerator enumerator, string id = null, Node connection = null)
    {
        var coroutine = new Coroutine(enumerator);
        coroutine.SetId(id);
        coroutine.SetConnection(connection);
        CoroutineHandler.Instance.AddCoroutine(coroutine);
        return coroutine;
    }

    public static Coroutine Start(Func<IEnumerator> enumerator, string id) => Start(enumerator(), id: id);
    public static Coroutine Start(Func<IEnumerator> enumerator, Node connection) => Start(enumerator(), connection: connection);
    public static Coroutine Start(Func<IEnumerator> enumerator, string id, Node connection) => Start(enumerator(), connection: connection, id: id);
    public static Coroutine Start(Func<IEnumerator> enumerator) => Start(enumerator());

    public static void Stop(Coroutine coroutine)
    {
        CoroutineHandler.Instance.RemoveCoroutine(coroutine);
    }

    public static void Stop(string id)
    {
        CoroutineHandler.Instance.RemoveCoroutineWithStringId(id);
    }

    public void UpdateFrame()
    {
        if (!TryMoveEnumerator(Enumerator))
        {
            HasEnded = true;
            HasCompleted = true;
        }
    }

    private bool TryMoveEnumerator(IEnumerator enumerator)
    {
        var new_enumerator = enumerator.Current != null ? enumerator.Current as IEnumerator : null;
        if (new_enumerator != null)
        {
            if (TryMoveEnumerator(new_enumerator))
            {
                return true;
            }
        }

        return enumerator.MoveNext();
    }

    public Coroutine SetConnection(Node node)
    {
        ConnectedNode = node;
        HasConnectedNode = node != null;
        return this;
    }

    public Coroutine SetId(string id)
    {
        StringId = id;
        return this;
    }

    public Coroutine SetRunWhilePaused(bool run_while_paused = true)
    {
        RunWhilePaused = run_while_paused;
        return this;
    }
}
