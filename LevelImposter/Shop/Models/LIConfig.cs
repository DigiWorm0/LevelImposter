using System;
using System.Collections.Generic;

namespace LevelImposter.Shop;

[Serializable]
public class LIConfig
{
    public string? LastMapJoined { get; set; }
    public Dictionary<string, float>? RandomWeights { get; set; }
}