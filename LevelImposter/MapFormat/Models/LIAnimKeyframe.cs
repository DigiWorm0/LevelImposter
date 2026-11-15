using System;

namespace LevelImposter.MapFormat.Models;

[Serializable]
public class LIAnimKeyframe
{
    //public int id { get; set; } // Not needed at runtime
    public float t { get; set; }
    public float value { get; set; }
    public string? nextCurve { get; set; }
}