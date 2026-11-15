namespace LevelImposter.RuntimeTests;

/// <summary>
///     A managed in-game test case. Implementations must be registered with
///     <see cref="RuntimeTestRegistry"/> before the chainloader finishes.
/// </summary>
public interface IRuntimeTest
{
    string Name { get; }

    void Start();

    /// <summary>
    ///     Advances the test. Return null while it is still running, or a result when complete.
    /// </summary>
    RuntimeTestResult? Tick();
}

public readonly record struct RuntimeTestResult(bool Passed, string Details);

public sealed class RuntimeTestRunReport
{
    public string RunId { get; init; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset StartedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAtUtc { get; set; }
    public List<RuntimeTestCaseReport> Tests { get; } = [];
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
}

public sealed class RuntimeTestCaseReport
{
    public string Name { get; init; } = "";
    public string Status { get; set; } = "Running";
    public string? Details { get; set; }
    public DateTimeOffset StartedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAtUtc { get; set; }
    public long? DurationMilliseconds { get; set; }
    public List<RuntimeTestLogEntry> LILoggerOutput { get; } = [];
}

public sealed class RuntimeTestLogEntry
{
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
    public string Level { get; init; } = "";
    public string Message { get; init; } = "";
}
