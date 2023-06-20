using System;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMinigameSprite
    {
        public Guid id { get; set; }
        public string type { get; set; }
        public string spriteData { get; set; }
    }
}
