using System;

namespace LevelImposter.Test;

/// <summary>
///     Runtime switches for automated testing.
///     Set <c>LI_TEST_MODE=1</c> when launching the in-process harness so the mod
///     avoids network side effects and filesystem watchers.
/// </summary>
public static class TestMode
{
    public const string ENVIRONMENT_VARIABLE = "LI_TEST_MODE";

    public static bool Enabled => ReadEnabled();

    private static bool ReadEnabled()
    {
        var value = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE);
        return value is "1" or "true" or "TRUE" or "True";
    }
}