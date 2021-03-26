using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Models
{
    [Serializable]
    class MapAsset
    {
        public string name;
        public string type;
        public string data;
        public float x;
        public float y;
        public float z;
        public float xScale;
        public float yScale;
        public float rotation;

        public MapCollider[] colliders;
    }
}
