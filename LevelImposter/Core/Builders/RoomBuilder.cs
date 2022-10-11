using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class RoomBuilder : IElemBuilder
    {
        private byte _roomId = 1;
        private static Dictionary<Guid, SystemTypes> _systemDB = new Dictionary<Guid, SystemTypes>();

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;

            SystemTypes systemType;
            do
            {
                systemType = (SystemTypes)_roomId;
                _roomId++;
            }
            while (systemType == SystemTypes.LowerEngine || systemType == SystemTypes.UpperEngine);

            PlainShipRoom shipRoom = obj.AddComponent<PlainShipRoom>();
            shipRoom.RoomId = systemType;
            shipRoom.roomArea = obj.GetComponentInChildren<Collider2D>();
            if (shipRoom.roomArea != null)
                shipRoom.roomArea.isTrigger = true;
            else
                LILogger.Warn(shipRoom.name + " is missing a collider");

            MapUtils.Rename(systemType, obj.name);
            _systemDB.Add(elem.id, systemType);
        }

        public void PostBuild()
        {
            MapUtils.Rename((SystemTypes)0, "Default Room");
            _roomId = 1;
        }

        public static SystemTypes GetSystem(Guid id)
        {
            return _systemDB.GetValueOrDefault(id);
        }

        public static SystemTypes[] GetAllSystems()
        {
            SystemTypes[] arr = new SystemTypes[_systemDB.Count];
            _systemDB.Values.CopyTo(arr, 0);
            return arr;
        }
    }
}
