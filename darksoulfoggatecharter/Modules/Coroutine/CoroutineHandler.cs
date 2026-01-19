using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CoroutineHandler : Node
{
    public static CoroutineHandler Instance => Singleton.TryGet<CoroutineHandler>(out var instance) ? instance : Create();

    private Dictionary<Guid, Coroutine> _coroutines = new();

    public static CoroutineHandler Create() =>
        Singleton.GetOrCreate<CoroutineHandler>($"{Paths.Modules}/Coroutine/{nameof(CoroutineHandler)}");

    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var paused = Scene.PauseLock.IsLocked;

        foreach (var coroutine in _coroutines.Values.ToList())
        {
            if (paused && !coroutine.RunWhilePaused)
            {
                continue;
            }

            if (CoroutineShouldBeRemoved(coroutine))
            {
                RemoveCoroutine(coroutine);
            }
            else
            {
                coroutine.UpdateFrame();
            }
        }
    }

    private bool CoroutineShouldBeRemoved(Coroutine coroutine)
    {
        if (coroutine.HasCompleted) return true;
        if (coroutine.HasEnded) return true;
        if (CoroutineHasLostConnection(coroutine)) return true;
        return false;
    }

    private bool CoroutineHasLostConnection(Coroutine coroutine)
    {
        if (!coroutine.HasConnectedNode) return false;
        if (coroutine.ConnectedNode == null) return true;
        if (!IsInstanceValid(coroutine.ConnectedNode)) return true;
        if (coroutine.ConnectedNode.IsQueuedForDeletion()) return true;
        return false;
    }

    public void AddCoroutine(Coroutine coroutine)
    {
        if (_coroutines.ContainsKey(coroutine.Id))
        {
            GD.PushWarning($"Attempted to add an existing coroutine with id: {coroutine.Id}");
            return;
        }

        RemoveCoroutineWithStringId(coroutine.StringId);

        coroutine.Id = Guid.NewGuid();
        _coroutines.Add(coroutine.Id, coroutine);
    }

    public void RemoveCoroutineWithStringId(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        var coroutine = _coroutines.Values.FirstOrDefault(x => x.StringId == id);
        RemoveCoroutine(coroutine);
    }

    public void RemoveCoroutine(Coroutine coroutine)
    {
        if (coroutine == null) return;

        coroutine.HasEnded = true;
        _coroutines.Remove(coroutine.Id);
    }
}
