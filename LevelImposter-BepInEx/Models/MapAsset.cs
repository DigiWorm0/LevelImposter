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
        public string spriteType;
        public string type;
        public long id;

        public float x;
        public float y;
        public float z;
        public float xScale;
        public float yScale;
        public float rotation;

        public bool flipX;
        public bool flipY;
        public bool onlyFromBottom;
        public long[] targetIds;

        public MapCollider[] colliders;
    }
}
