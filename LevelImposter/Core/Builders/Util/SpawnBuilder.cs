using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class SpawnBuilder : IElemBuilder
    {
        private bool _hasInitialSpawn = false;
        private bool _hasMeetingSpawn = false;
        private Vector2 _fallbackSpawn = new Vector2();

        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-spawn"))
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            // Ship Status
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;
            shipStatus.SpawnRadius = 1.55f;

            // Set Spawn Point
            Vector2 pos = obj.transform.position - new Vector3(0f, 0.3636f, 0f);
            _fallbackSpawn = pos;
            if (elem.type == "util-spawn1")
            {
                shipStatus.InitialSpawnCenter = pos;
                _hasInitialSpawn = true;
            }
            else if (elem.type == "util-spawn2")
            {
                shipStatus.MeetingSpawnCenter = pos;
                shipStatus.MeetingSpawnCenter2 = pos;
                _hasMeetingSpawn = true;
            }
            else
            {
                LILogger.Warn($"{elem.name} has an unknown spawn type");
            }
        }

        public void PostBuild()
        {
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;

            if (!_hasMeetingSpawn)
            {
                shipStatus.MeetingSpawnCenter = _fallbackSpawn;
                shipStatus.MeetingSpawnCenter2 = _fallbackSpawn;
            }
            if (!_hasInitialSpawn)
            {
                shipStatus.InitialSpawnCenter = _fallbackSpawn;
            }
        }
    }
}
