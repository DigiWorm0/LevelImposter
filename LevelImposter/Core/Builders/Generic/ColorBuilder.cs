using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Adds color to SpriteRenderers
    /// </summary>
    public class ColorBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (elem.properties.color != null && spriteRenderer != null)
                spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
        }

        public void PostBuild() { }
    }
}
