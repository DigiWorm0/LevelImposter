using System;

namespace LevelImposter.Core;

public class DelegateBoolValue(Func<bool> getValue) : IBoolValue
{
    public bool GetValue(int depth) => getValue();
}