using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelImposter.RuntimeTests;

/// <summary>
///     Explicit registry for in-game test cases. Test implementations are
///     managed classes and do not need IL2CPP type injection.
/// </summary>
public static class RuntimeTestRegistry
{
    private static readonly List<IRuntimeTest> Tests = [];

    public static IReadOnlyList<IRuntimeTest> RegisteredTests => Tests;

    public static void Register<T>() where T : IRuntimeTest, new()
    {
        if (Tests.Any(test => test.GetType() == typeof(T)))
            throw new InvalidOperationException($"Runtime test {typeof(T).FullName} is already registered.");

        Tests.Add(new T());
    }
}
