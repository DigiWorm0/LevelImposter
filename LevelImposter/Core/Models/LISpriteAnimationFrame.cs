using System;

namespace LevelImposter.Core;

[Serializable]
public class LISpriteAnimationFrame
{
    public Guid spriteID { get; set; }
    public float delay { get; set; }
}