using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class AdminMapBuilder : Builder
    {
        public const float ICON_OFFSET = -0.25f;
        private static List<CounterArea> counterAreaDB = new List<CounterArea>();
        private PoolableBehavior poolPrefab = null;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room" || elem.properties.isRoomAdminVisible == false)
                return;

            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            MapCountOverlay mapCountOverlay = mapBehaviour.countOverlay;

            // Prefab
            if (poolPrefab == null)
                poolPrefab = mapCountOverlay.CountAreas[0].pool.Prefab;

            // System
            SystemTypes systemType = RoomBuilder.GetSystem(elem.id);

            // Map Room
            float overlayScale = mapCountOverlay.transform.localScale.x;
            GameObject roomObj = new GameObject(elem.name);
            roomObj.transform.SetParent(mapCountOverlay.transform);
            roomObj.transform.localPosition = new Vector3(
                elem.x * MinimapBuilder.MAP_SCALE * (1 / overlayScale),
                elem.y * MinimapBuilder.MAP_SCALE * (1 / overlayScale) + ICON_OFFSET,
                -25.0f
            );

            CounterArea counterArea = roomObj.AddComponent<CounterArea>();
            counterArea.RoomType = systemType;
            counterArea.pool = roomObj.AddComponent<ObjectPoolBehavior>();
            counterArea.pool.Prefab = poolPrefab;

            counterAreaDB.Add(counterArea);

            mapCountOverlay.CountAreas = counterAreaDB.ToArray();
        }

        public void PostBuild()
        {
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            MapCountOverlay mapCountOverlay = mapBehaviour.countOverlay;

            while (mapCountOverlay.transform.childCount > counterAreaDB.Count)
                UnityEngine.Object.DestroyImmediate(mapCountOverlay.transform.GetChild(0).gameObject);

            counterAreaDB.Clear();    
        }
    }
}
