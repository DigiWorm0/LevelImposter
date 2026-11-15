using System;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;

namespace LevelImposter.RuntimeTests;

internal static class RuntimeTestReportWriter
{
    private const string ResultsPathEnvironmentVariable = "LI_RUNTIME_TEST_RESULTS";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void Write(RuntimeTestRunReport report, ManualLogSource log)
    {
        string? temporaryPath = null;
        try
        {
            var outputPath = GetOutputPath();
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputDirectory))
                throw new InvalidOperationException($"Invalid runtime test results path: {outputPath}");

            Directory.CreateDirectory(outputDirectory);
            temporaryPath = outputPath + ".tmp";
            var json = JsonSerializer.Serialize(report, SerializerOptions);
            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, outputPath, true);
            log.LogInfo($"Runtime test results written to {outputPath}");
        }
        catch (Exception exception)
        {
            log.LogError($"Failed to write runtime test results: {exception}");
            try
            {
                if (temporaryPath != null && File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch
            {
                // Best-effort cleanup; result reporting must not interrupt the game.
            }
        }
    }

    private static string GetOutputPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable(ResultsPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return Path.GetFullPath(configuredPath);

        return Path.Combine(Paths.ConfigPath, "LevelImposter.RuntimeTests", "results.json");
    }
}
