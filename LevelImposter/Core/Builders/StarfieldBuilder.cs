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

            // Prefab
            var prefab = AssetDB.GetObject("dec-rock4");
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();

            // Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                LILogger.Warn($"{elem.name} missing a sprite");
                return;
            }
            spriteRenderer.material = prefabRenderer.material;

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
            if (SpriteLoader.Instance == null)
            {
                LILogger.Warn("Spite Loader is not instantiated");
                return;
            }
            SpriteLoader.Instance.OnLoad += (LIElement loadedElem) =>
            {
                if (loadedElem.id != elem.id || liStars == null)
                    return;
                foreach (LIStar liStar in liStars)
                {
                    SpriteRenderer starRenderer = liStar.GetComponent<SpriteRenderer>();
                    starRenderer.sprite = spriteRenderer.sprite;
                    starRenderer.color = spriteRenderer.color;
                }
                liStars = null;
            };
        }

        public void PostBuild() { }
    }
}