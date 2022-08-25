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

        public Il2CppSystem.Collections.Generic.List<Vector2> GetPoints(bool wrap = false)
        {
            var list = new Il2CppSystem.Collections.Generic.List<Vector2>(points.Length);
            foreach (Point point in points)
                list.Add(new Vector2(point.x, -point.y));
            if (wrap && list.Count > 0)
                list.Add(list[0]);
            return list;
        }
    }
}
