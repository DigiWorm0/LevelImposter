using System;
using System.Diagnostics;

namespace LevelImposter.Test;

internal interface IProfilerType
{
    public IDisposable Measure(string name, string id);
}

internal sealed class NoOpProfilerType : IProfilerType
{
    public IDisposable Measure(string name, string id)
    {
        return new NoOpDisposable();
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

internal sealed class StopwatchProfilerType : IProfilerType
{
    public IDisposable Measure(string name, string id)
    {
        return new StopwatchDisposable(name, id);
    }

    private class StopwatchDisposable : IDisposable
    {
        private readonly string _id;
        private readonly string _name;
        private readonly Stopwatch _stopwatch = new();

        public StopwatchDisposable(string name, string id)
        {
            _name = name;
            _id = id;
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Profiler.AllResults.Add(new Profiler.ProfilerResult
            {
                Name = _name,
                ID = _id,
                DurationTicks = _stopwatch.ElapsedTicks
            });
        }
    }
}