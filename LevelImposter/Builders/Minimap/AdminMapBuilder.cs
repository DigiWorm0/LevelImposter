using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using LevelImposter.DB;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    public class AdminMapBuilder : IElemBuilder
    {
        public const float ICON_OFFSET = -0.25f;

        private List<CounterArea> _counterAreaDB = new();
        private PoolableBehavior? _poolPrefab = null;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room" || elem.properties.isRoomAdminVisible == false)
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            MapCountOverlay mapCountOverlay = mapBehaviour.countOverlay;

            // Prefab
            if (_poolPrefab == null)
                _poolPrefab = mapCountOverlay.CountAreas[0].pool.Prefab;

            // System
            SystemTypes systemType = RoomBuilder.GetSystem(elem.id);

            // Map Room
            float overlayScale = mapCountOverlay.transform.localScale.x * LIShipStatus.Instance.ShipStatus.MapScale;
            GameObject roomObj = new(elem.name);
            roomObj.transform.SetParent(mapCountOverlay.transform);
            roomObj.transform.localPosition = new Vector3(
                elem.x * (1 / overlayScale),
                elem.y * (1 / overlayScale) + ICON_OFFSET,
                -25.0f
            );

            CounterArea counterArea = roomObj.AddComponent<CounterArea>();
            counterArea.RoomType = systemType;
            counterArea.pool = roomObj.AddComponent<ObjectPoolBehavior>();
            counterArea.pool.Prefab = _poolPrefab;

            _counterAreaDB.Add(counterArea);

            mapCountOverlay.CountAreas = _counterAreaDB.ToArray();
        }

        public void PostBuild()
        {
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            MapCountOverlay mapCountOverlay = mapBehaviour.countOverlay;

            while (mapCountOverlay.transform.childCount > _counterAreaDB.Count)
                UnityEngine.Object.DestroyImmediate(mapCountOverlay.transform.GetChild(0).gameObject); 
        }
    }
}
