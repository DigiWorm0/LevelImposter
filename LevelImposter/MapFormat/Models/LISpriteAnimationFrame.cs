using System;

namespace LevelImposter.MapFormat.Models;

[Serializable]
public class LISpriteAnimationFrame
{
    public Guid spriteID { get; set; }
    public float delay { get; set; }
}