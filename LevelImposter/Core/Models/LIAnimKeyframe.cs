using System;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIAnimKeyframe
    {
        public Guid id { get; set; }
        public float t { get; set; }

        public string? nextCurve { get; set; }

        public float? x { get; set; }
        public float? y { get; set; }
        public float? z { get; set; }
        public float? xScale { get; set; }
        public float? yScale { get; set; }
        public float? rotation { get; set; }
    }
}
