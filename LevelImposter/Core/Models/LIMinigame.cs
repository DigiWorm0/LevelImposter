using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMinigame
    {
        public Guid id { get; set; }
        public string type { get; set; }
        public string spriteData { get; set; }
    }
}
