using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Adds color to SpriteRenderers
/// </summary>
internal static class ColorBuilder
{
    [ElementBuilder(Priority = Priority.VERY_HIGH)]
    public static void AddSpriteColor(LIElement element, GameObject gameObject)
    {
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
            spriteRenderer.color = element.properties.color?.ToUnity() ?? Color.white;
    }
}