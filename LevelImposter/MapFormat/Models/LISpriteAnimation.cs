using System;

namespace LevelImposter.MapFormat.Models;

[Serializable]
public class LISpriteAnimation
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string? type { get; set; }
    public LISpriteAnimationFrame[] frames { get; set; } = [];
}