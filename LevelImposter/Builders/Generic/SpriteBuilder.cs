using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LevelImposter.Builders
{
    /// <summary>
    /// Configures the SpriteRenderer on the GameObject
    /// </summary>
    public class SpriteBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.properties.spriteData == null)
                return;
            if (SpriteLoader.Instance == null)
                throw new Exception("SpriteLoader has not initialized");
            obj.AddComponent<SpriteRenderer>();
            SpriteLoader.Instance.LoadSpriteAsync(elem, obj);
        }

        public void PostBuild() { }
    }
}
