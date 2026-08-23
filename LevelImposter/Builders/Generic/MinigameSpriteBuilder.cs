using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Adds the MinigameSprites component (if needed)
/// </summary>
public static class MinigameSpriteBuilder
{
    [ElementBuilder]
    public static void AddMinigameSprites(LIElement element, GameObject gameObject)
    {
        if (element.properties.minigames == null && element.properties.minigameProps == null)
            return;
        var minigameSprites = gameObject.AddComponent<MinigameSprites>();
        minigameSprites.Init(element);
    }
}