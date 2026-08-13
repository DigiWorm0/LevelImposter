using System;

namespace LevelImposter.Core.Models;

[Serializable]
public class LICollider
{
    public Guid id { get; set; } = Guid.NewGuid();
    public bool blocksLight { get; set; }
    public bool isSolid { get; set; }
    public Point[] points { get; set; } = [];
}