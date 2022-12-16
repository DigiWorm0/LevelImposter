using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIUpdate
    {
        public string name { get; set; }
        public string tag { get; set; }
        public string downloadURL { get; set; }

        public bool isCurrent
        {
            get
            {
                return tag.Equals(LevelImposter.Version);
            }
        }
    }
}
