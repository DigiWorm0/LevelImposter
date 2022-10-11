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
        private static Dictionary<SystemTypes, MapRoom> _mapRoomDB = new Dictionary<SystemTypes, MapRoom>();

        private Sprite _commsBtnSprite;
        private Sprite _reactorBtnSprite;
        private Sprite _doorsBtnSprite;
        private Sprite _lightsBtnSprite;
        private Material _btnMat;

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-btn"))
                return;

            // Assets
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;
            if (_btnMat == null)
                GetAllAssets();
            
            // System
            SystemTypes systemType = 0;
            if (elem.properties.parent != null)
                systemType = RoomBuilder.GetSystem((Guid)elem.properties.parent);

            // Map Room
            MapRoom mapRoom;
            if (_mapRoomDB.ContainsKey(systemType))
            {
                mapRoom = _mapRoomDB[systemType];
            }
            else
            {
                GameObject roomObj = new GameObject(elem.name);
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
            GameObject sabButton = new GameObject(elem.name);
            sabButton.layer = (int)Layer.UI;
            sabButton.transform.SetParent(mapRoom.transform);
            sabButton.transform.localPosition = new Vector3(
                elem.x * MinimapBuilder.MinimapScale,
                elem.y * MinimapBuilder.MinimapScale,
                -25.0f
            );

            CircleCollider2D collider = sabButton.AddComponent<CircleCollider2D>();
            collider.radius = 0.425f;
            collider.isTrigger = true;

            SpriteRenderer btnRenderer = sabButton.AddComponent<SpriteRenderer>();
            if (mapRoom.special != null)
                LILogger.Warn("Only 1 sabotage is supported per room");
            mapRoom.special = btnRenderer;

            ButtonBehavior button = sabButton.AddComponent<ButtonBehavior>();
            Action btnAction = null;
            Sprite btnSprite = null;

            switch (elem.type) {
                case "sab-btnreactor":
                    btnSprite = _reactorBtnSprite;
                    btnAction = mapRoom.SabotageSeismic;
                    break;
                case "sab - btnoxygen":
                    btnSprite = _reactorBtnSprite; // TODO: Replace Me
                    btnAction = mapRoom.SabotageOxygen;
                    break;
                case "sab-btncomms":
                    btnSprite = _commsBtnSprite;
                    btnAction = mapRoom.SabotageComms;
                    break;
                case "sab-btnlights":
                    btnSprite = _lightsBtnSprite;
                    btnAction = mapRoom.SabotageLights;
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
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;

            while (infectedOverlay.transform.childCount > _mapRoomDB.Count)
                UnityEngine.Object.DestroyImmediate(infectedOverlay.transform.GetChild(0).gameObject);  
        }

        private void GetAllAssets()
        {
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;
            _commsBtnSprite = GetSprite(infectedOverlay, "Comms", "bomb"); // um...BOMB!?
            _reactorBtnSprite = GetSprite(infectedOverlay, "Laboratory", "meltdown");
            _doorsBtnSprite = GetSprite(infectedOverlay, "Office", "Doors");
            _lightsBtnSprite = GetSprite(infectedOverlay, "Electrical", "lightsOut");
            _btnMat = infectedOverlay.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().material;
        }

        private Sprite GetSprite(InfectedOverlay overlay, string parent, string child)
        {
            return overlay.transform.Find(parent).Find(child).GetComponent<SpriteRenderer>().sprite;
        }
    }
}
