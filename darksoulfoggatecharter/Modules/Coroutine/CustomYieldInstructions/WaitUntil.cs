using System;

public class WaitUntil : CustomYieldInstruction
{
    private Func<bool> ValidationFunc;
    public override bool KeepWaiting => !ValidationFunc();

    public WaitUntil(Func<bool> validationFunc)
    {
        ValidationFunc = validationFunc;
    }
}
