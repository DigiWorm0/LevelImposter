using System.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class LayerBuilder : IElemBuilder
{
    private static readonly Dictionary<string, Layer> TypeLayers = new()
    {
        { "util-ghostcollider", Layer.Default },
        { "util-binocularscollider", Layer.UICollider }
    };

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (!TypeLayers.TryGetValue(elem.type, out var layer))
            return;

        obj.layer = (int)layer;
    }
}