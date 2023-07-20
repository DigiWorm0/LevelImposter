#nullable enable
using System;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMapProperties
    {
        public string? bgColor { get; set; }
        public string? exileID { get; set; }
        public bool? showPingIndicator { get; set; }
        public bool? pixelArtMode { get; set; }
        public bool? preloadAllGIFs { get; set; }
        public bool? triggerLogging { get; set; }
    }
}
