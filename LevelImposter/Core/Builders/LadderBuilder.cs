using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Core
{
    class LadderBuilder : Builder
    {
        public static List<Ladder> AllLadders = new List<Ladder>();

        private byte ladderID = 0;
        private const float LADDER_Y_OFFSET = -0.4f;
        private static Dictionary<string, float> defaultLadderHeights = new Dictionary<string, float>
        {
            { "util-ladder1", 3.0f },
            { "util-ladder2", 1.5f }
        };

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-ladder"))
                return;

            UtilData utilData = AssetDB.utils[elem.type];
            Ladder topClone = utilData.GameObj.transform.GetChild(0).GetComponent<Ladder>();
            Ladder bottomClone = utilData.GameObj.transform.GetChild(1).GetComponent<Ladder>();

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            }
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Console
            float ladderHeight = elem.properties.ladderHeight == null ?
                defaultLadderHeights[elem.type] : (float)elem.properties.ladderHeight;
            
            GameObject topObj = new GameObject("LadderTop");
            topObj.transform.SetParent(obj.transform);
            topObj.transform.localPosition = new Vector3(0, ladderHeight + LADDER_Y_OFFSET, 0);
            topObj.AddComponent<BoxCollider2D>().isTrigger = true;
            GameObject bottomObj = new GameObject("LadderBottom");
            bottomObj.transform.SetParent(obj.transform);
            bottomObj.transform.localPosition = new Vector3(0, -ladderHeight + LADDER_Y_OFFSET, 0);
            bottomObj.AddComponent<BoxCollider2D>().isTrigger = true;

            Ladder topConsole = topObj.AddComponent<Ladder>();
            Ladder bottomConsole = bottomObj.AddComponent<Ladder>();
            topConsole.Id = ladderID++;
            topConsole.IsTop = true;
            topConsole.Destination = bottomConsole;
            topConsole.UseSound = topClone.UseSound;
            topConsole.Image = spriteRenderer;
            AllLadders.Add(topConsole);

            bottomConsole.Id = ladderID++;
            bottomConsole.IsTop = false;
            bottomConsole.Destination = topConsole;
            bottomConsole.UseSound = bottomClone.UseSound;
            bottomConsole.Image = spriteRenderer;
            AllLadders.Add(bottomConsole);
        }

        public void PostBuild() { }
    }
}