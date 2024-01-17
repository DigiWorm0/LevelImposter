using LevelImposter.Core;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class AdminMapBuilder : IElemBuilder
    {
        public const float ICON_OFFSET = -0.25f;

        private List<CounterArea> _counterAreaDB = new();
        private PoolableBehavior? _poolPrefab = null;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;

            // Check Admin
            bool isAdminVisible = elem.properties.isRoomAdminVisible ?? true;
            if (!isAdminVisible)
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            MapCountOverlay mapCountOverlay = mapBehaviour.countOverlay;

            // Prefab
            if (_poolPrefab == null)
                _poolPrefab = mapCountOverlay.CountAreas[0].pool.Prefab;

            // System
            SystemTypes systemType = RoomBuilder.GetSystem(elem.id);

            // Map Room
            float overlayScale = mapCountOverlay.transform.localScale.x * shipStatus.MapScale;
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
