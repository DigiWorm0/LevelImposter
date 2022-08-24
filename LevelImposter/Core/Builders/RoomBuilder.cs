using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class RoomBuilder : Builder
    {
        private byte roomId = 1;
        private static Dictionary<Guid, SystemTypes> systemDB = new Dictionary<Guid, SystemTypes>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;

            SystemTypes systemType;
            do
            {
                systemType = (SystemTypes)roomId;
                roomId++;
            }
            while (systemType == SystemTypes.LowerEngine || systemType == SystemTypes.UpperEngine);

            PlainShipRoom shipRoom = obj.AddComponent<PlainShipRoom>();
            shipRoom.RoomId = systemType;
            shipRoom.roomArea = obj.GetComponent<Collider2D>();
            if (shipRoom.roomArea != null)
                shipRoom.roomArea.isTrigger = true;
            else
                LILogger.Warn("Admin table will not function in " + shipRoom.name + " because it has no collider");

            MapUtils.Rename(systemType, obj.name);
            systemDB.Add(elem.id, systemType);
        }

        public void PostBuild()
        {
            systemDB.Clear();
            MapUtils.Rename((SystemTypes)0, "Default Room");
            roomId = 1;
        }

        public static SystemTypes GetSystem(Guid id)
        {
            return systemDB.GetValueOrDefault(id);
        }

        public static SystemTypes[] GetAllSystems()
        {
            SystemTypes[] arr = new SystemTypes[systemDB.Count];
            systemDB.Values.CopyTo(arr, 0);
            return arr;
        }
    }
}
