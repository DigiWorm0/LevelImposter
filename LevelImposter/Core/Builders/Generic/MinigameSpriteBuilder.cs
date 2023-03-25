using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Adds the MinigameSprites component (if needed)
    /// </summary>
    public class MinigameSpriteBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.properties.minigames == null && elem.properties.minigameProps == null)
                return;
            MinigameSprites minigameSprites = obj.AddComponent<MinigameSprites>();
            minigameSprites.Init(elem);
        }

        public void PostBuild() { }
    }
}
