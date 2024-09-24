using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Shop;

[Serializable]
public class GHAsset
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("size")] public int Size { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string? BrowserDownloadURL { get; set; }
}