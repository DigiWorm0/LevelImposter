using System.Collections.Generic;
using LevelImposter.Core.Models;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class LayerBuilder
{
    private static readonly Dictionary<string, Layer> TypeLayers = new()
    {
        { "util-ghostcollider", Layer.Default },
        { "util-binocularscollider", Layer.UICollider }
    };

    public static void ApplyLayer(LIElement element, GameObject gameObject)
    {
        if (!TypeLayers.TryGetValue(element.type, out var layer))
            return;

        gameObject.layer = (int)layer;
    }
}