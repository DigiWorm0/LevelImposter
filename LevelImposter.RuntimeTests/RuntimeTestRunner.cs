using BepInEx.Logging;
using LevelImposter.Logging;

namespace LevelImposter.RuntimeTests;

public static class RuntimeTestRunner
{
    private static ManualLogSource? _log;
    private static IRuntimeTest[] _tests = [];
    private static IRuntimeTest? _currentTest;
    private static int _nextTestIndex;
    private static int _passed;
    private static int _failed;
    private static bool _started;
    private static bool _finished;
    private static RuntimeTestRunReport? _report;
    private static RuntimeTestCaseReport? _currentTestReport;
    private static readonly object LogLock = new();

    public static void Initialize(ManualLogSource log)
    {
        _log = log;
        _tests = RuntimeTestRegistry.RegisteredTests.ToArray();
        _report = new RuntimeTestRunReport();
    }

    public static void StartAll()
    {
        if (_started)
            return;

        _started = true;
        if (_log == null)
            return;

        if (_report != null)
            RuntimeTestReportWriter.Write(_report, _log);

        if (_tests.Length == 0)
        {
            _log.LogWarning("No runtime tests are registered.");
            FinishRun();
            return;
        }

        StartNextTest();
    }

    public static void Tick()
    {
        if (!_started || _finished || _currentTest == null || _log == null)
            return;

        RuntimeTestResult? result;
        try
        {
            result = _currentTest.Tick();
        }
        catch (System.Exception exception)
        {
            result = new RuntimeTestResult(false, exception.ToString());
        }

        if (result.HasValue)
            FinishCurrentTest(result.Value);
    }

    private static void StartNextTest()
    {
        if (_log == null)
            return;

        if (_nextTestIndex >= _tests.Length)
        {
            FinishRun();
            return;
        }

        _currentTest = _tests[_nextTestIndex];
        _log.LogInfo($"Starting runtime test: {_currentTest.Name}");
        lock (LogLock)
        {
            _currentTestReport = new RuntimeTestCaseReport { Name = _currentTest.Name };
            _report?.Tests.Add(_currentTestReport);
            LILogger.OnLog += CaptureLILoggerOutput;
        }

        WriteReport();

        try
        {
            _currentTest.Start();
        }
        catch (System.Exception exception)
        {
            FinishCurrentTest(new RuntimeTestResult(false, exception.ToString()));
        }
    }

    private static void FinishCurrentTest(RuntimeTestResult result)
    {
        if (_log == null || _currentTest == null)
            return;

        LILogger.OnLog -= CaptureLILoggerOutput;
        lock (LogLock)
        {
            if (_currentTestReport != null)
            {
                _currentTestReport.Status = result.Passed ? "Passed" : "Failed";
                _currentTestReport.Details = result.Details;
                _currentTestReport.FinishedAtUtc = DateTimeOffset.UtcNow;
                _currentTestReport.DurationMilliseconds =
                    (long)(_currentTestReport.FinishedAtUtc.Value - _currentTestReport.StartedAtUtc).TotalMilliseconds;
            }

            _currentTestReport = null;
        }

        if (result.Passed)
        {
            _passed++;
            _log.LogInfo($"PASS: {_currentTest.Name}: {result.Details}");
        }
        else
        {
            _failed++;
            _log.LogError($"FAIL: {_currentTest.Name}: {result.Details}");
        }

        WriteReport();
        _currentTest = null;
        _nextTestIndex++;
        StartNextTest();
    }

    private static void CaptureLILoggerOutput(LILogLevel level, object data)
    {
        try
        {
            lock (LogLock)
            {
                if (_currentTestReport == null)
                    return;

                _currentTestReport.LILoggerOutput.Add(new RuntimeTestLogEntry
                {
                    Level = level.ToString(),
                    Message = ToMessage(data),
                    TimestampUtc = DateTimeOffset.UtcNow
                });
            }
        }
        catch
        {
            // The log-capture listener must never interfere with the game's logging path.
        }
    }

    private static string ToMessage(object data)
    {
        try
        {
            return data?.ToString() ?? "null";
        }
        catch (Exception exception)
        {
            return $"<message ToString threw {exception.GetType().FullName}>";
        }
    }

    private static void FinishRun()
    {
        _finished = true;
        if (_report != null)
        {
            _report.FinishedAtUtc = DateTimeOffset.UtcNow;
            _report.PassedCount = _passed;
            _report.FailedCount = _failed;
            WriteReport();
        }

        _log?.LogInfo($"Runtime test run finished: {_passed} passed, {_failed} failed.");
    }

    private static void WriteReport()
    {
        if (_report != null && _log != null)
        {
            lock (LogLock)
            {
                _report.PassedCount = _passed;
                _report.FailedCount = _failed;
                RuntimeTestReportWriter.Write(_report, _log);
            }
        }
    }
}
