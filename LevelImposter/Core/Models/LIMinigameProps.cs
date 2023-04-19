using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMinigameProps
    {
        public LIColor? reactorColorGood { get; set; }
        public LIColor? reactorColorBad { get; set; }
        public LIColor? lightsColorOn { get; set; }
        public LIColor? lightsColorOff { get; set; }
        public LIColor? fuelColor { get; set; }
        public LIColor? fuelBgColor { get; set; }
    }
}
