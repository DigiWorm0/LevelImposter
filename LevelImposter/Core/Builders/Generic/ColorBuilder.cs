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
            if (spriteRenderer)
                spriteRenderer.color = elem.properties.color?.ToUnity() ?? Color.white;
        }

        public void PostBuild() { }
    }
}
