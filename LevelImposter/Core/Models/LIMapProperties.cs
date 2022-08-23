#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMapProperties
    {
        public string? bgColor { get; set; }
        public Dictionary<string, string> resources { get; set; } = new();
    }
}
