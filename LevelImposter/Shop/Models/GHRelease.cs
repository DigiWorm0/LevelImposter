using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Shop;

[Serializable]
public class GHRelease
{
    [JsonPropertyName("tag_name")] public string? TagName { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("body")] public string? Body { get; set; }
    [JsonPropertyName("assets")] public GHAsset[]? Assets { get; set; }

    public override string ToString()
    {
        return Name ?? base.ToString() ?? "GHRelease";
    }
}