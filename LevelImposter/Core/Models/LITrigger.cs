using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LevelImposter.Core.Models;

[Serializable]
public class LITrigger
{
    [JsonPropertyName("id")] public string EventType { get; set; } = string.Empty;
    [JsonPropertyName("elemID")] public Guid? TargetID { get; set; }
    [JsonPropertyName("triggerID")] public string? TargetEventType { get; set; }
    [JsonPropertyName("properties")] public Dictionary<string, JsonElement>? Properties { get; set; }
}