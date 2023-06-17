using System;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMap : LIMetadata
    {
        public LIElement[] elements { get; set; }
        public LIMapProperties properties { get; set; }
    }
}
