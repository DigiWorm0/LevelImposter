using System;
using System.Collections.Generic;
using System.IO;
using LevelImposter.FileIO.API;

namespace LevelImposter.Test;

public static class Profiler
{
    private const string OUTPUT_FILE = "LevelImposter_Profiler.csv";

    public static readonly List<ProfilerResult> AllResults = [];

    private static readonly IProfilerType ProfilerType =
#if PROFILING
        new StopwatchProfilerType();
#else
        new NoOpProfilerType();
#endif

    public static IDisposable Measure(string name, string id)
    {
        return ProfilerType.Measure(name, id);
    }

    public static void DumpResults()
    {
        var filePath = FileAPI.GetPath(OUTPUT_FILE);
        using var writer = new StreamWriter(filePath);

        writer.WriteLine("DurationTicks,Name,ID");
        foreach (var result in AllResults)
            writer.WriteLine($"{result.DurationTicks},{result.Name},{result.ID}");
    }

    public struct ProfilerResult
    {
        public string Name;
        public string ID;
        public long DurationTicks;
    }
}