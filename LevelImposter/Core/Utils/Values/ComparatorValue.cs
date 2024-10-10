using System;
using LevelImposter.Builders;

namespace LevelImposter.Core;

public class ComparatorValue(Guid? value1ID, Guid? value2ID, ComparatorValue.Operation operation)
    : IBoolValue
{
    public enum Operation
    {
        AND,
        OR,
        XOR,
        NOT
    }

    public bool GetValue(int depth)
    {
        // Check for infinite loops
        if (depth > IBoolValue.MAX_DEPTH)
            throw new Exception("Infinite loop value loop detected. Check your value dependencies.");

        // Get values
        var value1 = ValueBuilder.GetBoolOfID(value1ID).GetValue(depth + 1);

        // Only get value2 if it's needed
        var value2 = operation != Operation.NOT && ValueBuilder.GetBoolOfID(value2ID).GetValue(depth + 1);

        // Perform operation
        return operation switch
        {
            Operation.AND => value1 && value2,
            Operation.OR => value1 || value2,
            Operation.XOR => value1 ^ value2,
            Operation.NOT => !value1,
            _ => false
        };
    }
}