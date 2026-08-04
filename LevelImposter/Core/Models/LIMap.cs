using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Core.Models;

[Serializable]
public class LIMap : LIMetadata
{
    // LIM2
    [JsonIgnore] public const int LIM_VERSION = 3;
    public LIElement[] elements { get; set; }
    public LIMapProperties properties { get; set; }
    public LISpriteAtlas[] spriteAtlases { get; set; }

    [JsonIgnore]
    public bool IsLegacy
    {
        get => v < LIM_VERSION;
        set => v = value ? 1 : LIM_VERSION;
    }

    [JsonIgnore] public MapAssetDB? MapAssetDB { get; set; }
}