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
        public const int LIM_VERSION = 2;
        [JsonIgnore]
        public bool isLegacy
        {
            get => v < LIM_VERSION;
            set => v = value ? 1 : LIM_VERSION;
        }
        [JsonIgnore]
        public MapAssetDB? mapAssetDB { get; set; }
    }
}
