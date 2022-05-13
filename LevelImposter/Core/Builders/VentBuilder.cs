using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using PowerTools;

namespace LevelImposter.Core
{
    class VentBuilder : Builder
    {
        private int id = 0;
        private bool hasSound = false;
        private static Dictionary<int, LIElement> ventElementDb = new Dictionary<int, LIElement>();
        private static Dictionary<Guid, Vent> ventComponentDb = new Dictionary<Guid, Vent>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-vent"))
                return;

            UtilData utilData = AssetDB.utils[elem.type];

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;

                if (elem.type == "util-vent1")
                {
                    SpriteAnim spriteAnimClone = utilData.GameObj.GetComponent<SpriteAnim>();
                    SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();
                    spriteAnim.Play(spriteAnimClone.m_defaultAnim, spriteAnimClone.Speed);
                }
            }
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Console
            VentCleaningConsole origConsole = utilData.GameObj.GetComponent<VentCleaningConsole>();
            VentCleaningConsole console = obj.AddComponent<VentCleaningConsole>();
            console.Image = spriteRenderer;
            console.AllowImpostor = false;
            console.checkWalls = false;
            console.GhostsIgnored = false;
            console.onlyFromBelow = false;
            console.onlySameRoom = false;
            console.usableDistance = 1;
            console.ImpostorDiscoveredSound = origConsole.ImpostorDiscoveredSound;
            if (elem.properties.range != null)
                console.usableDistance = (float)elem.properties.range;

            // Vent
            Vent ventData = utilData.GameObj.GetComponent<Vent>();
            Vent vent = obj.AddComponent<Vent>();
            vent.EnterVentAnim = ventData.EnterVentAnim;
            vent.ExitVentAnim = ventData.ExitVentAnim;
            vent.spreadAmount = ventData.spreadAmount;
            vent.spreadShift = ventData.spreadShift;
            vent.Offset = ventData.Offset;
            vent.Buttons = new UnhollowerBaseLib.Il2CppReferenceArray<ButtonBehavior>(0);
            vent.CleaningIndicators = new UnhollowerBaseLib.Il2CppReferenceArray<GameObject>(0);
            vent.Id = this.id;
            ShipStatus.Instance.AllVents = LIShipStatus.AddToArr(ShipStatus.Instance.AllVents, vent);

            // Arrows
            GameObject arrowPrefab = utilData.GameObj.transform.FindChild("Arrow").gameObject;
            for (int i = 0; i < 3; i++)
                GenerateArrow(arrowPrefab, vent, i);

            // Sounds
            if (!hasSound)
            {
                ShipStatus.Instance.VentEnterSound = AssetDB.ss["ss-skeld"].ShipStatus.VentEnterSound;
                ShipStatus.Instance.VentMoveSounds = AssetDB.ss["ss-skeld"].ShipStatus.VentMoveSounds;
                hasSound = true;
            }

            // Collider
            PolygonCollider2D polyCollider = obj.GetComponent<PolygonCollider2D>();
            if (polyCollider != null)
                polyCollider.isTrigger = true;

            // DB
            ventElementDb.Add(id, elem);
            ventComponentDb.Add(elem.id, vent);
            this.id++;
        }

        public void PostBuild()
        {
            id = 0;
            hasSound = false;

            foreach (var currentVent in ventElementDb)
            {
                Vent ventComponent = ventComponentDb[currentVent.Value.id];
                if (currentVent.Value.properties.leftVent != null)
                    ventComponent.Left = ventComponentDb[(Guid)currentVent.Value.properties.leftVent];
                if (currentVent.Value.properties.middleVent != null)
                    ventComponent.Center = ventComponentDb[(Guid)currentVent.Value.properties.middleVent];
                if (currentVent.Value.properties.rightVent != null)
                    ventComponent.Right = ventComponentDb[(Guid)currentVent.Value.properties.rightVent];
            }

            ventElementDb.Clear();
            ventComponentDb.Clear();
        }

        private void GenerateArrow(GameObject arrowPrefab, Vent vent, int dir)
        {
            SpriteRenderer cleaningClone = arrowPrefab.transform.FindChild("CleaningIndicator").GetComponent<SpriteRenderer>();
            SpriteRenderer arrowCloneSprite = arrowPrefab.GetComponent<SpriteRenderer>();
            BoxCollider2D arrowCloneBox = arrowPrefab.GetComponent<BoxCollider2D>();
            GameObject arrowObj = new GameObject("Arrow-" + dir);

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
            if (dir == 0)
                action = vent.ClickRight;
            else if (dir == 1)
                action = vent.ClickLeft;
            else
                action = vent.ClickCenter;
            arrowBtn.OnClick.AddListener(action);

            // Transform
            vent.Buttons = LIShipStatus.AddToArr(vent.Buttons, arrowBtn);
            arrowObj.transform.SetParent(vent.transform);
            arrowObj.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);
            arrowObj.active = false;

            // Cleaning Indicator
            GameObject cleaningIndicator = new GameObject("CleaningIndicator");
            cleaningIndicator.transform.SetParent(arrowObj.transform);
            cleaningIndicator.transform.localScale = new Vector3(0.45f, 0.45f, 1.0f);
            cleaningIndicator.transform.localPosition = new Vector3(-0.012f, 0, -0.02f);
            cleaningIndicator.layer = (int)Layer.UI;

            SpriteRenderer cleaningIndicatorSprite = cleaningIndicator.AddComponent<SpriteRenderer>();
            cleaningIndicatorSprite.sprite = cleaningClone.sprite;
            cleaningIndicatorSprite.material = cleaningClone.material;
            cleaningIndicator.active = false;
            vent.CleaningIndicators = LIShipStatus.AddToArr(vent.CleaningIndicators, cleaningIndicator);
        }
    }
}