using System;

namespace LevelImposter.Core;

[Serializable]
public class LISpriteAnimation
{
    public Guid id { get; set; }
    public LISpriteAnimationFrame[] frames { get; set; }
}