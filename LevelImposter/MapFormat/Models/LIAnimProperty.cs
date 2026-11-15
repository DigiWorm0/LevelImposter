using System;

namespace LevelImposter.MapFormat.Models;

[Serializable]
public class LIAnimProperty
{
    public LIAnimKeyframe[]? keyframes { get; set; }
}