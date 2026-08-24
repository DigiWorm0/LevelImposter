using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Core.Models;

[Serializable]
public class LIMetadata
{
    public int v { get; set; } = 0;
    public string id { get; set; } = Guid.NewGuid().ToString();
    public int? idVersion { get; set; }
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public string authorID { get; set; } = "";
    public string authorName { get; set; } = "";
    public bool isPublic { get; set; } = false;
    public bool isVerified { get; set; } = false;
    public long createdAt { get; set; } = 0;
    public string downloadURL { get; set; } = "";
    public string thumbnailURL { get; set; } = "";

    public Guid? remixOf { get; set; }

    public MapTarget MapTarget => MapTargetValue ?? MapTarget.Game;
    [JsonPropertyName("mapTarget")] public MapTarget? MapTargetValue { get; set; }

    /// <summary>
    ///     True if the map has a thumbnail available
    /// </summary>
    [JsonIgnore]
    public bool HasThumbnail => !string.IsNullOrEmpty(thumbnailURL);

    /// <summary>
    ///     True if the map has been uploaded to the workshop.
    ///     (Only maps with a valid GUID as ID are workshop maps)
    /// </summary>
    [JsonIgnore]
    public bool IsInWorkshop => Guid.TryParse(id, out _) && !string.IsNullOrEmpty(authorID);

    public override string ToString()
    {
        return $"{name}[{id}]";
    }
}