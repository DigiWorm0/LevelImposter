using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LevelImposter.CoreTests.Infrastructure;

public sealed class WorkshopManifest
{
    [JsonPropertyName("schemaVersion")] public int SchemaVersion { get; init; }
    [JsonPropertyName("maps")] public List<WorkshopMapEntry> Maps { get; init; } = [];

    public static WorkshopManifest Load()
    {
        var json = File.ReadAllText(TestPaths.WorkshopManifestPath);
        return JsonSerializer.Deserialize<WorkshopManifest>(json) ??
               throw new InvalidDataException("Failed to deserialize workshop map manifest.");
    }
}

public sealed class WorkshopMapEntry
{
    [JsonPropertyName("id")] public string Id { get; init; } = "";
    [JsonPropertyName("fileName")] public string FileName { get; init; } = "";
    [JsonPropertyName("name")] public string Name { get; init; } = "";
    [JsonPropertyName("authorName")] public string AuthorName { get; init; } = "";
    [JsonPropertyName("mapVersion")] public int MapVersion { get; init; }
    [JsonPropertyName("format")] public string? Format { get; init; }
    [JsonPropertyName("sizeBytes")] public long? SizeBytes { get; init; }
    [JsonPropertyName("sha256")] public string? Sha256 { get; init; }

    public string CachePath => Path.Combine(TestPaths.WorkshopCacheDirectory, FileName);
}
