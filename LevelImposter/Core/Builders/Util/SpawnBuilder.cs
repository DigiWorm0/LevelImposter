using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class SpawnBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-spawn"))
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new Exception("ShipStatus not found");

            // Set Spawn Point
            Vector2 pos = obj.transform.position - new Vector3(0, 0);
            ShipStatus shipStatus = LIShipStatus.Instance.ShipStatus;
            shipStatus.SpawnRadius = 1.55f;
            if (elem.type == "util-spawn1")
            {
                shipStatus.InitialSpawnCenter = pos;
            }
            else if (elem.type == "util-spawn2")
            {
                shipStatus.MeetingSpawnCenter = pos;
                shipStatus.MeetingSpawnCenter2 = pos;
            }
            else
            {
                LILogger.Warn($"{elem.name} has an unknown spawn type");
            }
        }

        public void PostBuild() { }
    }
}
