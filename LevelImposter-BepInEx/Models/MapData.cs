using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Models
{
    [Serializable]
    class MapData
    {
        public string name;
        public string btn;
        public string map;
        public MapType exile;
        public MapAsset[] objs;
    }
}
