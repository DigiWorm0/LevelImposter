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
        public bool isClosed;
        public Vector2[] points;

        public Il2CppSystem.Collections.Generic.List<Vector2> GetPoints(float xScale = 1.0f, float yScale = 1.0f)
        {
            var list = new Il2CppSystem.Collections.Generic.List<Vector2>(points.Length);
            
            foreach (Vector2 point in points)
                list.Add(new Vector2(point.x / xScale, -point.y / yScale));
            if (points.Length > 0 && isClosed)
                list.Add(new Vector2(points[0].x / xScale, -points[0].y / yScale));

            return list;
        }
    }
}
