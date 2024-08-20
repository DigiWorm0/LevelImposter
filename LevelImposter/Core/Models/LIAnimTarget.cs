using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIAnimTarget
    {
        public Guid id { get; set; }
        public Dictionary<string, LIAnimProperty> properties { get; set; }
    }
}
