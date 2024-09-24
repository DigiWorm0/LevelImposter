using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Shop;

[Serializable]
public class LIUpdate
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("tag")] public string? Tag { get; set; }
    [JsonPropertyName("downloadURL")] public string? DownloadURL { get; set; }

    public bool IsCurrent => Tag?.Equals(LevelImposter.DisplayVersion) ?? false;
}