using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIElement
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float xScale { get; set; }
        public float yScale { get; set; }
        public float rotation { get; set; }

        public LIProperties properties { get; set; }
    }
}
