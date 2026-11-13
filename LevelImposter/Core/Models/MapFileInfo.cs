using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Core.Models;

/// <summary>
///     Represents the metadata of a LevelImposter map file.
/// </summary>
[Serializable]
public class MapFileInfo
{
    [JsonPropertyName("id")] public string ID { get; set; } = Guid.NewGuid().ToString();
    [JsonPropertyName("name")] public string Name { get; set; } = "";

    public int v { get; set; } = 0;
    public int? idVersion { get; set; }
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
    public bool IsInWorkshop => Guid.TryParse(ID, out _) && !string.IsNullOrEmpty(authorID);

    /// <summary>
    ///     Returns a string representation of the full file path of this map, if one exists in the filesystem.
    /// </summary>
    [JsonIgnore]
    public string? FilePath { get; set; }

    /// <summary>
    ///     Converts the map to a string representation in the form of "name[id]"
    /// </summary>
    /// <returns>String representation of the map</returns>
    public override string ToString()
    {
        return $"{Name}[{ID}]";
    }
}