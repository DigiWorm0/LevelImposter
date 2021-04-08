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
                list.Add(point);

            return list;
        }
    }
}
