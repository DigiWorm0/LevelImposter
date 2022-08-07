using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class SabMapBuilder : Builder
    {
        private static Dictionary<SystemTypes, MapRoom> mapRoomDB = new Dictionary<SystemTypes, MapRoom>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("sab-"))
                return;

            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;

            // Is anyone going to talk about the fact that comms is literally called "bomb" in Unity?
            Sprite commsSprite = GetSprite(infectedOverlay, "Comms", "bomb"); // um...BOMB!?
            Sprite reactorSprite = GetSprite(infectedOverlay, "Laboratory", "meltdown");
            Sprite doorsSprite = GetSprite(infectedOverlay, "Office", "Doors");
            Sprite lightsSprite = GetSprite(infectedOverlay, "Electrical", "lightsOut");
            Material buttonMat = infectedOverlay.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().material;

            // System
            SystemTypes systemType = 0;
            if (elem.properties.parent != null)
                systemType = RoomBuilder.GetSystem((Guid)elem.properties.parent);

            // Map Room
            MapRoom mapRoom;
            if (mapRoomDB.ContainsKey(systemType))
            {
                mapRoom = mapRoomDB[systemType];
            }
            else
            {
                GameObject roomObj = new GameObject(elem.name);
                roomObj.transform.SetParent(infectedOverlay.transform);
                roomObj.transform.localPosition = Vector3.zero;

                mapRoom = roomObj.AddComponent<MapRoom>();
                mapRoom.Parent = infectedOverlay;
                mapRoom.room = systemType;

                mapRoomDB.Add(systemType, mapRoom);

                MapRoom[] rooms = new MapRoom[mapRoomDB.Count];
                mapRoomDB.Values.CopyTo(rooms, 0);
                infectedOverlay.rooms = rooms;
            }

            // Button
            GameObject sabButton = new GameObject(elem.name);
            sabButton.layer = (int)Layer.UI;
            sabButton.transform.SetParent(mapRoom.transform);
            sabButton.transform.localPosition = new Vector3(
                elem.x * MinimapBuilder.MAP_SCALE,
                elem.y * MinimapBuilder.MAP_SCALE,
                -25.0f
            );

            CircleCollider2D collider = sabButton.AddComponent<CircleCollider2D>();
            collider.radius = 0.425f;
            collider.isTrigger = true;

            SpriteRenderer renderer = sabButton.AddComponent<SpriteRenderer>();
            renderer.sprite = lightsSprite;
            renderer.material = buttonMat;
            if (mapRoom.special != null)
                LILogger.Warn("Only 1 sabotage is supported per room. Sabotage buttons may experience strange behaviour.");
            mapRoom.special = renderer;
            ButtonBehavior button = sabButton.AddComponent<ButtonBehavior>();
            Action action = mapRoom.SabotageLights;

            if (elem.type == "sab-reactorleft" || elem.type == "sab-reactorright")
            {
                renderer.sprite = reactorSprite;
                action = mapRoom.SabotageSeismic;
            }
            else if (elem.type == "sab-oxygen1" || elem.type == "sab-oxygen2")
            {
                renderer.sprite = reactorSprite; // TODO: Fix Me
                action = mapRoom.SabotageOxygen;
            }
            else if (elem.type == "sab-comms")
            {
                renderer.sprite = commsSprite;
                action = mapRoom.SabotageComms;
            }
            button.OnClick.AddListener(action);

        }

        public void PostBuild()
        {
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;

            while (infectedOverlay.transform.childCount > mapRoomDB.Count)
                UnityEngine.Object.DestroyImmediate(infectedOverlay.transform.GetChild(0).gameObject);
            
            mapRoomDB.Clear();    
        }

        private Sprite GetSprite(InfectedOverlay overlay, string parent, string child)
        {
            return overlay.transform.Find(parent).Find(child).GetComponent<SpriteRenderer>().sprite;
        }
    }
}
