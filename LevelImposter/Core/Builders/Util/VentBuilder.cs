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
        private Dictionary<int, LIElement> _ventElementDb = new();
        private Dictionary<Guid, Vent> _ventComponentDb = new();
        private int _ventID = 0;
        private bool _hasVentSound = false;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-vent"))
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new Exception("ShipStatus not found");

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabConsole = prefab.GetComponent<VentCleaningConsole>();
            var prefabVent = prefab.GetComponent<Vent>();
            var prefabArrow = prefab.transform.FindChild("Arrow").gameObject;

            // Skeld ShipStatus
            var skeldShip = AssetDB.GetObject("ss-skeld");
            var skeldShipStatus = skeldShip?.GetComponent<ShipStatus>();

            // Default Sprite
            bool isAnim = elem.type == "util-vent1";
            SpriteRenderer spriteRenderer = MapUtils.CloneSprite(obj, prefab, isAnim);

            // Console
            VentCleaningConsole console = obj.AddComponent<VentCleaningConsole>();
            console.Image = spriteRenderer;
            console.ImpostorDiscoveredSound = prefabConsole.ImpostorDiscoveredSound;
            console.TaskTypes = prefabConsole.TaskTypes;
            console.ValidTasks = prefabConsole.ValidTasks;
            if (elem.properties.range != null)
                console.usableDistance = (float)elem.properties.range;

            // Vent
            Vent vent = obj.AddComponent<Vent>();
            vent.EnterVentAnim = prefabVent.EnterVentAnim;
            vent.ExitVentAnim = prefabVent.ExitVentAnim;
            vent.spreadAmount = prefabVent.spreadAmount;
            vent.spreadShift = prefabVent.spreadShift;
            vent.Offset = prefabVent.Offset;
            vent.Buttons = new Il2CppReferenceArray<ButtonBehavior>(0);
            vent.CleaningIndicators = new Il2CppReferenceArray<GameObject>(0);
            vent.Id = _ventID;

            // Arrows
            GameObject arrowParent = new GameObject($"{obj.name}_arrows");
            arrowParent.transform.SetParent(obj.transform);
            arrowParent.transform.localPosition = Vector3.zero;
            for (int i = 0; i < 3; i++)
                GenerateArrow(prefabArrow, vent, i).transform.SetParent(arrowParent.transform);

            // Sounds
            if (!_hasVentSound)
            {
                shipStatus.VentEnterSound = skeldShipStatus?.VentEnterSound;
                shipStatus.VentMoveSounds = skeldShipStatus?.VentMoveSounds;
                _hasVentSound = true;
            }

            // Colliders
            MapUtils.CreateDefaultColliders(obj, prefab);

            // DB
            _ventElementDb.Add(_ventID, elem);
            _ventComponentDb.Add(elem.id, vent);
            _ventID++;
        }

        public void PostBuild()
        {
            _ventID = 0;
            _hasVentSound = false;

            foreach (var currentVent in _ventElementDb)
            {
                Vent? ventComponent = GetVentComponent(currentVent.Value.id);
                if (ventComponent == null)
                    continue;
                if (currentVent.Value.properties.leftVent != null)
                    ventComponent.Left = GetVentComponent((Guid)currentVent.Value.properties.leftVent);
                if (currentVent.Value.properties.middleVent != null)
                    ventComponent.Center = GetVentComponent((Guid)currentVent.Value.properties.middleVent);
                if (currentVent.Value.properties.rightVent != null)
                    ventComponent.Right = GetVentComponent((Guid)currentVent.Value.properties.rightVent);
            }
        }

        /// <summary>
        /// Gets a vent component from the vent component db
        /// </summary>
        /// <param name="id">GUID of the vent</param>
        /// <returns>Vent component or null if not found</returns>
        private Vent? GetVentComponent(Guid id)
        {
            bool exists = _ventComponentDb.TryGetValue(id, out Vent? ventComponent);
            if (!exists)
                return null;
            return ventComponent;
        }

        /// <summary>
        /// Generates the vent arrow buttons
        /// </summary>
        /// <param name="arrowPrefab">Prefab to steal from</param>
        /// <param name="vent">Vent target</param>
        /// <param name="dir">Direction to point arrow</param>
        private GameObject GenerateArrow(GameObject arrowPrefab, Vent vent, int dir)
        {
            SpriteRenderer cleaningClone = arrowPrefab.transform.FindChild("CleaningIndicator").GetComponent<SpriteRenderer>();
            SpriteRenderer arrowCloneSprite = arrowPrefab.GetComponent<SpriteRenderer>();
            BoxCollider2D arrowCloneBox = arrowPrefab.GetComponent<BoxCollider2D>();
            GameObject arrowObj = new("Arrow-" + dir);

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
            arrowObj.transform.localScale = new Vector3(
                0.4f,
                0.4f,
                1.0f
            );
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

            return arrowObj;
        }
    }
}