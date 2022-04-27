using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIProperties
    {
        public Guid? leftVent { get; set; }
        public Guid? middleVent { get; set; }
        public Guid? rightVent { get; set; }

        public bool? onlyFromBelow { get; set; }
        public float? range { get; set; }

        public string spriteData { get; set; }
        public LICollider[] colliders { get; set; }
    }
}
