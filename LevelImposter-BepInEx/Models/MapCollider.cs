using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Models
{
    [Serializable]
    class MapCollider
    {
        public bool blocksLight;
        public Point[] points;
    }
}
