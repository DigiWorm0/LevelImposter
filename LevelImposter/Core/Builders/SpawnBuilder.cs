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

            Vector2 pos = new Vector2(elem.x, elem.y - LIShipStatus.Y_OFFSET);
            if (elem.type == "util-spawn1")
            {
                ShipStatus.Instance.InitialSpawnCenter = pos;
            }
            else if (elem.type == "util-spawn2")
            {
                ShipStatus.Instance.MeetingSpawnCenter = pos;
                ShipStatus.Instance.MeetingSpawnCenter2 = pos;
            }
        }

        public void PostBuild() { }
    }
}
