using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using PowerTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class VentBuilder : Builder
    {
        private PolusHandler polus;
        private bool hasSounds = false;
        private int id;
        private static Dictionary<long, Vent> ventDb;
        private static Dictionary<long, long[]> targetDb;

        public VentBuilder(PolusHandler polus)
        {
            this.polus = polus;
            this.id = 0;
            ventDb = new Dictionary<long, Vent>();
            targetDb = new Dictionary<long, long[]>();
        }

        public bool PreBuild(MapAsset asset)
        {
            if (!asset.type.StartsWith("util-vent"))
                return true;
            UtilData utilData = AssetDB.utils[asset.type];

            // Object
            GameObject obj = new GameObject(asset.type);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            spriteRenderer.material = utilData.SpriteRenderer.material;
            obj.layer = (int)Layer.ShortObjects;

            // Animator (If Applicable)
            if (asset.type == "util-vent1")
            {
                SpriteAnim spriteAnimClone = utilData.GameObj.GetComponent<SpriteAnim>();
                SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();

                spriteAnim.Play(spriteAnimClone.m_defaultAnim, spriteAnimClone.Speed);
                //spriteAnim.m_clipPairList = spriteAnimClone.m_clipPairList;
            }


            // Vent
            Vent ventData = utilData.GameObj.GetComponent<Vent>();
            Vent vent = obj.AddComponent<Vent>();
            vent.EnterVentAnim = ventData.EnterVentAnim;
            vent.ExitVentAnim = ventData.ExitVentAnim;
            vent.spreadAmount = ventData.spreadAmount;
            vent.spreadShift = ventData.spreadShift;
            vent.Offset = ventData.Offset;
            vent.myRend = spriteRenderer;
            vent.Buttons = new UnhollowerBaseLib.Il2CppReferenceArray<ButtonBehavior>(0);
            vent.Id = this.id;
            this.id++;

            // Arrow Buttons
            GameObject arrowClone = utilData.GameObj.transform.FindChild("Arrow").gameObject;
            SpriteRenderer arrowCloneSprite = arrowClone.GetComponent<SpriteRenderer>();
            BoxCollider2D arrowCloneBox = arrowClone.GetComponent<BoxCollider2D>();
            for(int i = 0; i < asset.targetIds.Length && i < 3; i++)
            {
                long targetId = asset.targetIds[i];
                if (targetId < 0)
                    continue;
                if (MapHandler.GetById(targetId) == null)
                    continue;

                GameObject arrowObj = new GameObject("Arrow" + targetId);

                // Sprite
                SpriteRenderer arrowSprite = arrowObj.AddComponent<SpriteRenderer>();
                arrowSprite.sprite = arrowCloneSprite.sprite;
                arrowSprite.material = arrowCloneSprite.material;
                arrowObj.layer = (int)Layer.UI;

                // Box Collider
                BoxCollider2D arrowBox = arrowObj.AddComponent<BoxCollider2D>();
                arrowBox.size = arrowCloneBox.size;
                arrowBox.offset = arrowCloneBox.offset;
                arrowBox.isTrigger = true;

                // Button
                ButtonBehavior arrowBtn = arrowObj.AddComponent<ButtonBehavior>();
                arrowBtn.OnMouseOver = new UnityEvent();
                arrowBtn.OnMouseOut = new UnityEvent();
                Action action;
                if (i == 0)
                    action = vent.ClickRight;
                else if (i == 1)
                    action = vent.ClickLeft;
                else
                    action = vent.ClickCenter;
                arrowBtn.OnClick.AddListener(action);

                // Transform
                vent.Buttons = AssetHelper.AddToArr(vent.Buttons, arrowBtn);
                arrowObj.transform.SetParent(obj.transform);
                arrowObj.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);
                arrowObj.active = false;
            }

            // Box Collider
            BoxCollider2D origBox = utilData.GameObj.GetComponent<BoxCollider2D>();
            BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
            box.size = origBox.size;
            box.offset = origBox.offset;
            box.isTrigger = true;

            // Colliders
            AssetHelper.BuildColliders(asset, obj);

            // Polus
            ventDb.Add(asset.id, vent);
            targetDb.Add(asset.id, asset.targetIds);
            polus.shipStatus.AllVents = AssetHelper.AddToArr(polus.shipStatus.AllVents, vent);
            polus.Add(obj, asset);

            // Sounds
            if (!hasSounds)
            {
                polus.shipStatus.VentEnterSound = AssetDB.ss["ss-skeld"].ShipStatus.VentEnterSound;
                polus.shipStatus.VentMoveSounds = AssetDB.ss["ss-skeld"].ShipStatus.VentMoveSounds;
                hasSounds = true;
            }
            
            return true;
        }

        public bool PostBuild()
        {
            foreach (var targetData in targetDb)
            {
                Vent vent = ventDb[targetData.Key];
                long[] targets = targetData.Value;

                if (targets.Length < 3)
                    continue;
                if (ventDb.ContainsKey(targets[0]))
                    vent.Right = ventDb[targets[0]];
                if (ventDb.ContainsKey(targets[1]))
                    vent.Left = ventDb[targets[1]];
                if (ventDb.ContainsKey(targets[2]))
                    vent.Center = ventDb[targets[2]];
            }

            return true;
        }
    }
}
