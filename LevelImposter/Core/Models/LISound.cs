using System;

namespace LevelImposter.Core
{
    [Serializable]
    public class LISound
    {
        public Guid id { get; set; }
        public string? type { get; set; }
        public string? data { get; set; }
        public float volume { get; set; }
        public bool isPreset { get; set; }
    }
}
