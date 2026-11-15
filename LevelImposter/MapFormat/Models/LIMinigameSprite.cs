using System;

namespace LevelImposter.MapFormat.Models;

[Serializable]
public class LIMinigameSprite
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string type { get; set; } = "";
    public Guid? spriteID { get; set; }

    // Legacy
    [Obsolete("Use spriteID instead")] public string? spriteData { get; set; }
}