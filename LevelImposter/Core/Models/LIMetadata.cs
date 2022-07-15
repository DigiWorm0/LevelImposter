using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMetadata
    {
        public System.Guid id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string authorID { get; set; }
        public bool isPublic { get; set; }
        public string storageURL { get; set; }
    }
}
