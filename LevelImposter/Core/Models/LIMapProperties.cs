#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMapProperties
    {
        public string? bgColor { get; set; }
        public string? exileID { get; set; }
        public bool? showPingIndicator { get; set; }
        public bool? pixelArtMode { get; set; }
    }
}
