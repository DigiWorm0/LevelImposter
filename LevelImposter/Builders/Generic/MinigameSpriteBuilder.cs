﻿using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders
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
