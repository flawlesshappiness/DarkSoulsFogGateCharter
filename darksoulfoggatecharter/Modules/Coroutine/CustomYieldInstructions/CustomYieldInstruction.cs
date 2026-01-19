using System.Collections;

public class CustomYieldInstruction : IEnumerator
{
    public object Current => null;

    public bool MoveNext() => KeepWaiting;

    public void Reset()
    {
    }

    public virtual bool KeepWaiting { get; }
}
