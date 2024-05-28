#nullable enable
using System;
using System.Collections.Generic;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIProperties
    {
        // Generic
        public Guid? parent { get; set; }
        public LICollider[]? colliders { get; set; }

        // Triggers
        public LITrigger[]? triggers { get; set; }
        public float? triggerTime { get; set; }
        public bool? triggerClientSide { get; set; }
        public LIColor? highlightColor { get; set; }
        public int? triggerCount { get; set; }
        public bool? triggerLoop { get; set; }
        public bool? createDeadBody { get; set; }

        // Sprite
        public Guid? spriteID { get; set; }
        public bool? noShadows { get; set; }
        public bool? noShadowsBehaviour { get; set; }
        public LIColor? color { get; set; }
        public bool? loopGIF { get; set; }

        // Shake
        public float? shakeAmount { get; set; }
        public float? shakePeriod { get; set; }

        // Legacy
        [Obsolete("Use spriteID instead")]
        public string? spriteData { get; set; }

        // One-Way Colliders
        public bool? isImposterIgnored { get; set; }

        // Towels
        public float? towelPickupCount { get; set; }

        // Spores
        public LIColor? gasColor { get; set; }

        // Decontamination
        public Guid? doorA { get; set; }
        public Guid? doorB { get; set; }
        public float? deconDuration { get; set; }

        // Scroll
        public float? scrollingXSpeed { get; set; }
        public float? scrollingYSpeed { get; set; }

        // Custom Text
        public Dictionary<string, string>? customText { get; set; }

        // Minigame
        public LIMinigameSprite[]? minigames { get; set; }
        public LIMinigameProps? minigameProps { get; set; }

        // Sound
        public LISound[]? sounds { get; set; }
        public int? soundPriority { get; set; }

        // Door
        public string? doorType { get; set; }
        public bool? isDoorInteractable { get; set; }

        // Vent
        public Guid? leftVent { get; set; }
        public Guid? middleVent { get; set; }
        public Guid? rightVent { get; set; }

        // Teleporter
        public Guid? teleporter { get; set; }
        public bool? preserveOffset { get; set; }
        public bool? isGhostEnabled { get; set; }

        // Camera
        public float? camXOffset { get; set; }
        public float? camYOffset { get; set; }
        public float? camZoom { get; set; }

        // Display
        public int? displayWidth { get; set; }
        public int? displayHeight { get; set; }

        // Console
        public bool? onlyFromBelow { get; set; }
        public bool? checkCollision { get; set; }
        public float? range { get; set; }
        public float? sporeRange { get; set; }

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
        public float? sabDuration { get; set; }

        // Room
        public bool? isRoomNameVisible { get; set; }
        public bool? isRoomAdminVisible { get; set; }
        public bool? isRoomUIVisible { get; set; }

        // Spawn
        public bool? spawnDummies { get; set; }

        // Meeting
        [Obsolete("Use meetingBackgroundID instead")]
        public string? meetingBackground { get; set; }
        public Guid? meetingBackgroundID { get; set; }

        // Minimap
        public float? minimapScale { get; set; }
        public bool? imposterOnly { get; set; }

    }
}
