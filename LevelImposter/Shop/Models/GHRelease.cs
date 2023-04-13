using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    [Serializable]
    public class GHRelease
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public GHAsset[] assets { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
