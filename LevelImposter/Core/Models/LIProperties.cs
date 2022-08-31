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
        public LICollider[]? colliders { get; set; }

        // Sprite
        public string? spriteData { get; set; }
        public bool? noShadows { get; set; }
        public bool? noShadowsBehaviour { get; set; }

        // Sound
        public LISound[]? sounds { get; set; }
        public int? soundPriority { get; set; }

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
        
        // Platform
        public float? platformXOffset { get; set; }
        public float? platformYOffset { get; set; }
        public float? platformXEntranceOffset { get; set; }
        public float? platformYEntranceOffset { get; set; }
        public float? platformXExitOffset { get; set; }
        public float? platformYExitOffset { get; set; }

        // Star Field
        public int? starfieldCount { get; set; }
        public float? starfieldMinSpeed { get; set; }
        public float? starfieldMaxSpeed { get; set; }
        public float? starfieldHeight { get; set; }
        public float? starfieldLength { get; set; }

        // Floating
        public float? floatingHeight { get; set; }
        public float? floatingSpeed { get; set; }

        // Tasks
        public string? description { get; set; }
        public string? taskLength { get; set; }

        // Room
        public bool? isRoomNameVisible { get; set; }
        public bool? isRoomAdminVisible { get; set; }

        // Minimap
        public float? minimapScale { get; set; }

    }
}
