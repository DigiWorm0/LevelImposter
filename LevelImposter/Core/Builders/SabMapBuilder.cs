using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class SabMapBuilder : IElemBuilder
    {
        private static Dictionary<SystemTypes, MapRoom> _mapRoomDB = null;
        private readonly Dictionary<string, string> _sabToLegacyTypes = new Dictionary<string, string> {
            { "sab-electric", "sab-btnlights" },
            { "sab-reactorleft", "sab-btnreactor" },
            { "sab-oxygen", "sab-btnoxygen" },
            { "sab-comms", "sab-btncomms" },
        };

        private Sprite _commsBtnSprite = null;
        private Sprite _reactorBtnSprite = null;
        private Sprite _oxygenBtnSprite = null;
        private Sprite _doorsBtnSprite = null;
        private Sprite _lightsBtnSprite = null;
        private Material _btnMat = null;

        private bool _hasSabConsoles = false;
        private bool _hasSabButtons = false;

        public SabMapBuilder()
        {
            _mapRoomDB = new Dictionary<SystemTypes, MapRoom>();
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-") || elem.type.StartsWith("sab-door"))
                return;
            _hasSabConsoles = true;

            if (!elem.type.StartsWith("sab-btn"))
                return;
            _hasSabButtons = true;

            // Assets
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;
            if (_btnMat == null)
                GetAllAssets();
            
            // System
            SystemTypes systemType = RoomBuilder.GetParentOrDefault(elem);

            // Map Room
            MapRoom mapRoom;
            if (_mapRoomDB.ContainsKey(systemType))
            {
                mapRoom = _mapRoomDB[systemType];
            }
            else
            {
                GameObject roomObj = new(elem.name);
                roomObj.transform.SetParent(infectedOverlay.transform);
                roomObj.transform.localPosition = Vector3.zero;

                mapRoom = roomObj.AddComponent<MapRoom>();
                mapRoom.Parent = infectedOverlay;
                mapRoom.room = systemType;

                _mapRoomDB.Add(systemType, mapRoom);

                MapRoom[] rooms = new MapRoom[_mapRoomDB.Count];
                _mapRoomDB.Values.CopyTo(rooms, 0);
                infectedOverlay.rooms = rooms;
            }

            // Button
            float mapScale = LIShipStatus.Instance.ShipStatus.MapScale;
            GameObject sabButton = new(elem.name);
            sabButton.layer = (int)Layer.UI;
            sabButton.transform.SetParent(mapRoom.transform);
            sabButton.transform.localPosition = new Vector3(
                elem.x / mapScale,
                elem.y / mapScale,
                -25.0f
            );
            sabButton.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1);
            sabButton.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);

            CircleCollider2D collider = sabButton.AddComponent<CircleCollider2D>();
            collider.radius = 0.425f;
            collider.isTrigger = true;

            SpriteRenderer btnRenderer = sabButton.AddComponent<SpriteRenderer>();
            if (mapRoom.special != null)
                LILogger.Warn("Only 1 sabotage button is supported per room");

            ButtonBehavior button = sabButton.AddComponent<ButtonBehavior>();
            Action btnAction = null;
            Sprite btnSprite = null;

            switch (elem.type) {
                case "sab-btnreactor":
                    btnSprite = _reactorBtnSprite;
                    btnAction = mapRoom.SabotageSeismic;
                    mapRoom.special = btnRenderer;
                    break;
                case "sab-btnoxygen":
                    btnSprite = _oxygenBtnSprite; // TODO: Replace Me
                    btnAction = mapRoom.SabotageOxygen;
                    mapRoom.special = btnRenderer;
                    break;
                case "sab-btncomms":
                    btnSprite = _commsBtnSprite;
                    btnAction = mapRoom.SabotageComms;
                    mapRoom.special = btnRenderer;
                    break;
                case "sab-btnlights":
                    btnSprite = _lightsBtnSprite;
                    btnAction = mapRoom.SabotageLights;
                    mapRoom.special = btnRenderer;
                    break;
                case "sab-btndoors":
                    btnSprite = _doorsBtnSprite;
                    btnAction = mapRoom.SabotageDoors;
                    mapRoom.door = btnRenderer;
                    break;
                default:
                    LILogger.Error($"{elem.name} has unknown sabotage button type: {elem.type}");
                    return;
            }

            btnRenderer.sprite = btnSprite;
            btnRenderer.material = _btnMat;
            button.OnClick.AddListener(btnAction);
            SpriteRenderer origRenderer = obj.GetComponent<SpriteRenderer>();
            if (origRenderer != null)
            {
                btnRenderer.sprite = origRenderer.sprite;
                btnRenderer.color = origRenderer.color;
            }
        }


        public void PostBuild()
        {
            if (_hasSabConsoles && !_hasSabButtons)
            {
                LILogger.Warn("Map does not include sabotage buttons.\n(Placeholder buttons are depricated and may have weird or unexpected behaviour.)");
                BuildAllLegacy();
            }

            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;

            while (infectedOverlay.transform.childCount > _mapRoomDB.Count)
                UnityEngine.Object.DestroyImmediate(infectedOverlay.transform.GetChild(0).gameObject);
        }

        private void BuildAllLegacy()
        {
            LIMap currentMap = LIShipStatus.Instance.CurrentMap;

            foreach (LIElement elem in currentMap.elements)
            {
                if (_sabToLegacyTypes.TryGetValue(elem.type, out string btnType))
                {
                    LIElement tempElem = new()
                    {
                        id = Guid.NewGuid(),
                        name = elem.name,
                        type = btnType,
                        x = elem.x,
                        y = elem.y,
                        z = elem.z,
                        xScale = elem.xScale,
                        yScale = elem.yScale,
                        rotation = elem.rotation,
                        properties = new()
                        {
                            parent = elem.properties.parent
                        }
                    };
                    LIShipStatus.Instance.AddElement(tempElem);
                }
            }
        }

        /// <summary>
        /// Collects all necessary sprites and assets for map
        /// </summary>
        private void GetAllAssets()
        {
            // Polus
            ShipStatus polusShip = AssetDB.Ships["ss-polus"].ShipStatus;
            InfectedOverlay polusOverlay = polusShip.MapPrefab.infectedOverlay;

            _commsBtnSprite = GetSprite(polusOverlay, "Comms", "bomb"); // um...BOMB!?
            _reactorBtnSprite = GetSprite(polusOverlay, "Laboratory", "meltdown");
            _doorsBtnSprite = GetSprite(polusOverlay, "Office", "Doors");
            _lightsBtnSprite = GetSprite(polusOverlay, "Electrical", "lightsOut");
            _btnMat = polusOverlay.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().material;

            // Mira
            ShipStatus miraShip = AssetDB.Ships["ss-mira"].ShipStatus;
            InfectedOverlay miraOverlay = miraShip.MapPrefab.infectedOverlay;

            _oxygenBtnSprite = GetSprite(miraOverlay, "LifeSupp", "bomb"); // Another bomb?
        }

        /// <summary>
        /// Searches an object for a sprite in a parent and child
        /// </summary>
        /// <param name="overlay">Object to search</param>
        /// <param name="parent">Parent object name</param>
        /// <param name="child">Child object name</param>
        /// <returns>Sprite attatched to SpriteRenderer</returns>
        private Sprite GetSprite(InfectedOverlay overlay, string parent, string child)
        {
            return overlay.transform.Find(parent).Find(child).GetComponent<SpriteRenderer>().sprite;
        }
    }
}
