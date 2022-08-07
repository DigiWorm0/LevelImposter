using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [Serializable]
    public class Point
    {
        public float x { get; set; }
        public float y { get; set; }

        public Vector2 toVector()
        {
            return new Vector2(x, y);
        }
    }
}
