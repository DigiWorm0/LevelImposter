using HarmonyLib;
using LevelImposter.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using PowerTools;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.Core
{
    class VentBuilder : IElemBuilder
    {
        private static Dictionary<int, LIElement> _ventElementDb = null;
        private static Dictionary<Guid, Vent> _ventComponentDb = null;

        private int _ventID = 0;
        private bool _hasVentSound = false;

        public VentBuilder()
        {
            _ventElementDb = new Dictionary<int, LIElement>();
            _ventComponentDb = new Dictionary<Guid, Vent>();
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-vent"))
                return;

            UtilData utilData = AssetDB.Utils[elem.type];

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            GIFAnimator gifAnimator = obj.GetComponent<GIFAnimator>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
                if (elem.type == "util-vent1")
                {
                    SpriteAnim spriteAnimClone = utilData.GameObj.GetComponent<SpriteAnim>();
                    SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();
                    spriteAnim.Play(spriteAnimClone.m_defaultAnim, spriteAnimClone.Speed);
                }
            }
            else if (gifAnimator != null)
            {
                gifAnimator.Stop();
            }
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Console
            VentCleaningConsole origConsole = utilData.GameObj.GetComponent<VentCleaningConsole>();
            VentCleaningConsole console = obj.AddComponent<VentCleaningConsole>();
            console.Image = spriteRenderer;
            console.ImpostorDiscoveredSound = origConsole.ImpostorDiscoveredSound;
            console.TaskTypes = origConsole.TaskTypes;
            console.ValidTasks = origConsole.ValidTasks;
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
            vent.Buttons = new Il2CppReferenceArray<ButtonBehavior>(0);
            vent.CleaningIndicators = new Il2CppReferenceArray<GameObject>(0);
            vent.Id = this._ventID;

            // Arrows
            GameObject arrowPrefab = utilData.GameObj.transform.FindChild("Arrow").gameObject;
            for (int i = 0; i < 3; i++)
                GenerateArrow(arrowPrefab, vent, i);

            // Sounds
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;
            if (!_hasVentSound)
            {
                shipStatus.VentEnterSound = AssetDB.Ships["ss-skeld"].ShipStatus.VentEnterSound;
                shipStatus.VentMoveSounds = AssetDB.Ships["ss-skeld"].ShipStatus.VentMoveSounds;
                _hasVentSound = true;
            }

            // Collider
            PolygonCollider2D[] solidColliders = obj.GetComponentsInChildren<PolygonCollider2D>();
            for (int i = 0; i < solidColliders.Length; i++)
                solidColliders[i].isTrigger = true;
            if (solidColliders.Length <= 0)
            {
                BoxCollider2D boxCollider = obj.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(elem.xScale, elem.yScale);
                boxCollider.offset = new Vector2(elem.xScale / 2, elem.yScale / 2);
                boxCollider.isTrigger = true;
            }

            // DB
            _ventElementDb.Add(_ventID, elem);
            _ventComponentDb.Add(elem.id, vent);
            this._ventID++;
        }

        public void PostBuild()
        {
            _ventID = 0;
            _hasVentSound = false;

            foreach (var currentVent in _ventElementDb)
            {
                Vent ventComponent = _ventComponentDb[currentVent.Value.id];
                if (currentVent.Value.properties.leftVent != null)
                    ventComponent.Left = _ventComponentDb[(Guid)currentVent.Value.properties.leftVent];
                if (currentVent.Value.properties.middleVent != null)
                    ventComponent.Center = _ventComponentDb[(Guid)currentVent.Value.properties.middleVent];
                if (currentVent.Value.properties.rightVent != null)
                    ventComponent.Right = _ventComponentDb[(Guid)currentVent.Value.properties.rightVent];
            }
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
            vent.Buttons = MapUtils.AddToArr(vent.Buttons, arrowBtn);
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
            vent.CleaningIndicators = MapUtils.AddToArr(vent.CleaningIndicators, cleaningIndicator);
        }
    }
}