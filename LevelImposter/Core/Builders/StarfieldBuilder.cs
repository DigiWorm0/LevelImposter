using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    class StarfieldBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-starfield")
                return;

            // Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            LISpriteLoader spriteLoader = obj.GetComponent<LISpriteLoader>();
            if (spriteRenderer != null)
            {
                spriteRenderer.material = AssetDB.Decor["dec-rock4"].SpriteRenderer.material;
            }
            else
            {
                LILogger.Warn(elem.name + " missing a sprite");
                return;
            }

            // Star Prefab
            GameObject starPrefab = UnityEngine.Object.Instantiate(obj);
            LIStar prefabComp = starPrefab.AddComponent<LIStar>();

            int count = elem.properties.starfieldCount ?? 20;
            LIStar[] liStars = new LIStar[count];
            for (int i = 0; i < count; i++)
            {
                LIStar liStar = UnityEngine.Object.Instantiate(prefabComp, obj.transform);
                liStar.Init(elem);
                liStars[i] = liStar;
            }

            // Clones
            spriteLoader.OnLoad.AddListener((Action<Sprite>)((Sprite sprite) =>
            {
                foreach (LIStar liStar in liStars)
                    liStar.GetComponent<SpriteRenderer>().sprite = sprite;
                UnityEngine.Object.Destroy(starPrefab);
            }));
        }

        public void PostBuild() { }
    }
}