using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMetadata
    {
        public int v { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string authorID { get; set; }
        public string authorName { get; set; }
        public bool isPublic { get; set; }
        public bool isVerified { get; set; }
        public long createdAt { get; set; }
        public string[] downloadURL { get; set; }
    }
}
