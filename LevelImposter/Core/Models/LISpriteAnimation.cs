using System;

namespace LevelImposter.Core.Models;

[Serializable]
public class LISpriteAnimation
{
    public Guid id { get; set; }
    public string? type { get; set; }
    public LISpriteAnimationFrame[] frames { get; set; }
}