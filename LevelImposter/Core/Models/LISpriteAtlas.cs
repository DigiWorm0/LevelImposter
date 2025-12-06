using System;

namespace LevelImposter.Core;

[Serializable]
public class LISpriteAtlas
{
    public Guid id { get; set; }
    public Guid assetID { get; set; }
    public int x { get; set; }
    public int y { get; set; }
    public int w { get; set; }
    public int h { get; set; }
}