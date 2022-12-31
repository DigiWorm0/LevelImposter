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
            if (spriteRenderer == null)
            {
                LILogger.Warn(elem.name + " missing a sprite");
                return;
            }
            spriteRenderer.material = AssetDB.Decor["dec-rock4"].SpriteRenderer.material;

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
            UnityEngine.Object.Destroy(starPrefab);

            // Clones
            SpriteLoader.Instance.OnLoad += (LIElement loadedElem) =>
            {
                if (loadedElem.id != elem.id)
                    return;

                foreach (LIStar liStar in liStars)
                    liStar.GetComponent<SpriteRenderer>().sprite = spriteRenderer.sprite;
            };
        }

        public void PostBuild() { }
    }
}