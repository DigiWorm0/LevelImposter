using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class RoomBuilder : Builder
    {
        private int roomId = 0;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;

            SystemTypes systemType = (SystemTypes)roomId;
            roomId++;

            PlainShipRoom shipRoom = obj.AddComponent<PlainShipRoom>();
            shipRoom.RoomId = systemType;
            shipRoom.roomArea = obj.GetComponent<Collider2D>();
            shipRoom.roomArea.isTrigger = true;
        }

        public void PostBuild()
        {
            roomId = 0;
        }
    }
}
