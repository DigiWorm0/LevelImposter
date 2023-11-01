using System;
using System.Text.Json.Serialization;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMap : LIMetadata
    {
        public LIElement[] elements { get; set; }
        public LIMapProperties properties { get; set; }

        // LIM2
        [JsonIgnore]
        public bool isLegacy => v <= 1;
        [JsonIgnore]
        public MapAssetDB? mapAssetDB { get; set; }
    }
}
