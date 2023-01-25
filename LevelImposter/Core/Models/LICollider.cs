using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [Serializable]
    public class LICollider
    {
        public Guid id { get; set; }
        public bool blocksLight { get; set; }
        public bool isSolid { get; set; }
        public Point[] points { get; set; }
    }
}
