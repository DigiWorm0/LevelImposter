using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    class StarfieldBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-starfield")
                return;

            // Sprite
            GameObject prefab = GameObject.Instantiate(obj, LIShipStatus.Instance.transform);
            LIStar prefabStar = prefab.AddComponent<LIStar>();
            SpriteRenderer prefabRenderer = obj.GetComponent<SpriteRenderer>();
            if (prefabRenderer != null)
            {
                prefabRenderer.material = AssetDB.dec["dec-rock4"].SpriteRenderer.material;
            }
            else
            {
                LILogger.Warn("Star Fields do not work without a cooresponding sprite");
                return;
            }

            // Stars
            int count = 20;
            if (elem.properties.starfieldCount != null)
                count = (int)elem.properties.starfieldCount;
            for (int i = 0; i < count; i++)
            {
                GameObject starObj = GameObject.Instantiate(prefab, obj.transform);
                starObj.name = "Star " + i;
                LIStar starComp = starObj.GetComponent<LIStar>();
                starComp.Init(elem);
            }

            // Disable Obj
            obj.GetComponent<SpriteRenderer>().enabled = false;
            GameObject.Destroy(prefab);
        }

        public void PostBuild() { }
    }
}