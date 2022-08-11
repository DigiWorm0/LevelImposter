using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class NoShadowBuilder : Builder
    {
        private Material noShadowMat = null;
        private Material defaultMat = null;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!(elem.type.StartsWith("dec-") || elem.type == "util-blank"))
                return;

            if (noShadowMat == null)
            {
                noShadowMat = AssetDB.dec["dec-rock5"].SpriteRenderer.material;
                defaultMat = AssetDB.dec["dec-rock4"].SpriteRenderer.material;
            }

            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                if (elem.properties.noShadows == true)
                {
                    spriteRenderer.material = noShadowMat;

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
                    spriteRenderer.material = defaultMat;
                }
            }
        }

        public void PostBuild() { }
    }
}