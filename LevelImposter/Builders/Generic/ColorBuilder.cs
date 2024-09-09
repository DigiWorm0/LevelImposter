using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

/// <summary>
///     Adds color to SpriteRenderers
/// </summary>
public class ColorBuilder : IElemBuilder
{
    public void OnBuild(LIElement elem, GameObject obj)
    {
        var spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
            spriteRenderer.color = elem.properties.color?.ToUnity() ?? Color.white;
    }
}