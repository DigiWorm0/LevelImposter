using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Shop;

[Serializable]
public class LICallback<T>
{
    [JsonPropertyName("v")] public int Version { get; set; }
    [JsonPropertyName("error")] public string? Error { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }
}