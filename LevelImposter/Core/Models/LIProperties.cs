#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIProperties
    {
        // Generic
        public Guid? parent { get; set; }
        public string? spriteData { get; set; }
        public LICollider[]? colliders { get; set; }

        // Vent
        public Guid? leftVent { get; set; }
        public Guid? middleVent { get; set; }
        public Guid? rightVent { get; set; }

        // Camera
        public float? camXOffset { get; set; }
        public float? camYOffset { get; set; }
        public float? camZoom { get; set; }

        // Console
        public bool? onlyFromBelow { get; set; }
        public float? range { get; set; }

        // Ladder
        public float? ladderHeight { get; set; }

        // Tasks
        public string? description { get; set; }
        public string? taskLength { get; set; }

        

    }
}
