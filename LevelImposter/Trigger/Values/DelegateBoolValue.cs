using System;

namespace LevelImposter.Trigger.Values;

public class DelegateBoolValue(Func<bool> getValue) : IBoolValue
{
    public bool GetValue(int depth)
    {
        return getValue();
    }
}