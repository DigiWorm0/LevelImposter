using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class NoShadowBuilder : IElemBuilder
    {
        private Material _noShadowMat = null;
        private Material _defaultMat = null;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!(elem.type.StartsWith("dec-") || elem.type.StartsWith("util-blank")))
                return;

            if (_noShadowMat == null)
            {
                _noShadowMat = AssetDB.Decor["dec-rock5"].SpriteRenderer.material;
                _defaultMat = AssetDB.Decor["dec-rock4"].SpriteRenderer.material;
            }

            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                if (elem.properties.noShadows == true)
                {
                    spriteRenderer.material = _noShadowMat;

                    if (elem.properties.noShadowsBehaviour == true)
                    {
                        NoShadowBehaviour behaviour = obj.AddComponent<NoShadowBehaviour>();
                        behaviour.rend = spriteRenderer;

                        Collider2D[] shadows = obj.GetComponentsInChildren<Collider2D>();
                        for (int i = 0; i < shadows.Length; i++)
                        {
                            if (shadows[i].gameObject != obj)
                            {
                                behaviour.hitOverride = shadows[i];
                                return;
                            }
                        }
                    }
                }
                else
                {
                    spriteRenderer.material = _defaultMat;
                }
            }
        }

        public void PostBuild() { }
    }
}