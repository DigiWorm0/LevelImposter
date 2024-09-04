using System;
using System.Collections.Generic;

namespace LevelImposter.Core;

[Serializable]
public class LIAnimTarget
{
    public Guid id { get; set; }
    public Dictionary<string, LIAnimProperty> properties { get; set; }
}