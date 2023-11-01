using System;

namespace LevelImposter.Core
{
    [Serializable]
    public class LISound
    {
        public Guid id { get; set; }
        public string? type { get; set; }
        public float volume { get; set; }
        public Guid? dataID { get; set; }
        public string? channel { get; set; }
        public bool isPreset { get; set; }
        public string? presetID { get; set; }

        // Legacy
        [Obsolete("Use dataID instead")]
        public string? data { get; set; }
    }
}
