using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class SpawnBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (!elem.type.StartsWith("util-spawn"))
                return;

            Vector2 pos = obj.transform.position - new Vector3(0, LIShipStatus.Y_OFFSET);
            ShipStatus shipStatus = LIShipStatus.Instance.shipStatus;
            if (elem.type == "util-spawn1")
            {
                shipStatus.InitialSpawnCenter = pos;
            }
            else if (elem.type == "util-spawn2")
            {
                shipStatus.MeetingSpawnCenter = pos;
                shipStatus.MeetingSpawnCenter2 = pos;
            }
        }

        public void PostBuild() { }
    }
}
