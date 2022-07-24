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
            GameObject buttonPrefab = infectedOverlay.transform.GetChild(0).GetChild(0).gameObject;

            SystemTypes systemType = 0;
            if (elem.properties.parent != null)
                systemType = RoomBuilder.GetSystem((Guid)elem.properties.parent);

            MapRoom mapRoom;
            if (mapRoomDB.ContainsKey(systemType))
            {
                mapRoom = mapRoomDB[systemType];
            }
            else
            {
                GameObject roomObj = new GameObject("Room " + ((int)systemType));
                roomObj.transform.SetParent(infectedOverlay.transform);

                mapRoom = roomObj.AddComponent<MapRoom>();
                mapRoom.Parent = infectedOverlay;
                mapRoom.room = systemType;

                mapRoomDB.Add(systemType, mapRoom);
            }

            GameObject sabButton = new GameObject(elem.name);
            sabButton.transform.SetParent(mapRoom.transform);
            sabButton.transform.localPosition = new Vector3(
                elem.x * 0.2f * 1.25f,
                elem.y * 0.2f * 1.25f,
                -25.0f
            );

            CircleCollider2D collider = sabButton.AddComponent<CircleCollider2D>();
            collider.radius = 0.425f;
            collider.isTrigger = true;

            SpriteRenderer renderer = sabButton.AddComponent<SpriteRenderer>();
            renderer.sprite = buttonPrefab.GetComponent<SpriteRenderer>().sprite;

            ButtonBehavior button = sabButton.AddComponent<ButtonBehavior>();
            Action action = mapRoom.SabotageLights;
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
    }
}
