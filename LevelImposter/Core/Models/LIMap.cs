using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMap
    {
        public System.Guid id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string authorName { get; set; }
        public DateTime creationDate { get; set; }
        public int downloadCount { get; set; }
        public LIElement[] elements { get; set; }
    }
}
