using System.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

internal class LayerBuilder : IElemBuilder
{
    private readonly Dictionary<string, Layer> _typeLayers = new()
    {
        { "util-ghostcollider", Layer.Default },
        { "util-binocularscollider", Layer.UICollider }
    };

    public void Build(LIElement elem, GameObject obj)
    {
        if (!_typeLayers.ContainsKey(elem.type))
            return;

        obj.layer = (int)_typeLayers[elem.type];
    }

    public void PostBuild()
    {
    }
}