#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIProperties
    {
        public Guid? parent { get; set; }

        public Guid? leftVent { get; set; }
        public Guid? middleVent { get; set; }
        public Guid? rightVent { get; set; }

        public bool? onlyFromBelow { get; set; }
        public float? range { get; set; }
        public string? description { get; set; }

        public float? camXOffset { get; set; }
        public float? camYOffset { get; set; }
        public float? camZoom { get; set; }

        public string? spriteData { get; set; }
        public LICollider[]? colliders { get; set; }
    }
}
