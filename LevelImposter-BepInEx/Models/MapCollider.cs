using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Models
{
    [Serializable]
    class MapCollider
    {
        public bool blocksLight;
        public Vector2[] points;

        public Il2CppSystem.Collections.Generic.List<Vector2> GetPoints()
        {
            var list = new Il2CppSystem.Collections.Generic.List<Vector2>(points.Length);
            
            foreach (Vector2 point in points)
                list.Add(new Vector2(point.x, -point.y));
            if (points.Length > 0)
                list.Add(new Vector2(points[0].x, -points[0].y));

            return list;
        }
    }
}
