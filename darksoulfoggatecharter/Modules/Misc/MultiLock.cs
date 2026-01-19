using System.Collections.Generic;

public class MultiLock
{
    private List<string> _locks = new();

    public event System.Action OnLocked;
    public event System.Action OnFree;

    public bool IsLocked => _locks.Count > 0;
    public bool IsFree => _locks.Count == 0;

    public void Clear()
    {
        _locks.Clear();
        OnFree?.Invoke();
    }

    public void AddLock(string key)
    {
        if (_locks.Contains(key)) return;
        var was_locked = IsLocked;
        _locks.Add(key);

        if (!was_locked)
        {
            OnLocked?.Invoke();
        }
    }

    public void RemoveLock(string key)
    {
        if (!_locks.Contains(key)) return;
        var was_locked = IsLocked;
        _locks.Remove(key);

        if (was_locked && IsFree)
        {
            OnFree?.Invoke();
        }
    }

    public void SetLock(string key, bool locked)
    {
        if (locked)
        {
            AddLock(key);
        }
        else
        {
            RemoveLock(key);
        }
    }
}
