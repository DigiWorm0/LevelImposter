using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [Serializable]
    public class LITrigger
    {
        public string id { get; set; }
        public Guid? elemID { get; set; }
        public string triggerID { get; set; }
    }
}
