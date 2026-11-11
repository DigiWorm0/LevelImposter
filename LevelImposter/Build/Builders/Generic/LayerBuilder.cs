using System.Collections.Generic;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Build.Builders.Generic;

/// <summary>
///     Moves the specified object type to the specified layer
/// </summary>
internal static class LayerBuilder
{
    private static readonly Dictionary<string, Layer> ElementTypeToLayer = new()
    {
        { "util-ghostcollider", Layer.Default },
        { "util-binocularscollider", Layer.UICollider }
    };

    [ElementBuilder(Priority = Priority.VERY_HIGH)]
    public static void ApplyLayer(LIElement element, GameObject gameObject)
    {
        if (!ElementTypeToLayer.TryGetValue(element.type, out var layer))
            return;

        gameObject.layer = (int)layer;
    }
}