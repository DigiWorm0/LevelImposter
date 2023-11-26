using LevelImposter.Core;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders
{
    class LayerBuilder : IElemBuilder
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

        public void PostBuild() { }
    }
}