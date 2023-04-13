using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class RoomBuilder : IElemBuilder
    {
        private static Dictionary<Guid, SystemTypes> _systemDB = new();
        private byte _roomId = 1;

        public RoomBuilder()
        {
            _systemDB.Clear();
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;
            if (LIShipStatus.Instance?.ShipStatus == null)
                throw new MissingShipException();

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
            else if ((elem.properties.isRoomAdminVisible ?? true) || (elem.properties.isRoomNameVisible ?? true))
                LILogger.Warn($"{shipRoom.name} is missing a collider");

            LIShipStatus.Instance.Renames.Add(systemType, obj.name);
            _systemDB.Add(elem.id, systemType);
        }

        public void PostBuild()
        {
            if (LIShipStatus.Instance == null)
                throw new MissingShipException();
            LIShipStatus.Instance.Renames.Add((SystemTypes)0, "Default Room");
            _roomId = 1;
        }

        /// <summary>
        /// Gets the SystemTypes associated with a specific util-room ID
        /// </summary>
        /// <param name="id">ID of the util-room object</param>
        /// <returns>Associated SystemTypes value</returns>
        public static SystemTypes GetSystem(Guid id)
        {
            return _systemDB.GetValueOrDefault(id);
        }

        /// <summary>
        /// Gets the SystemTypes of the parent of an object
        /// </summary>
        /// <param name="element">Object to read</param>
        /// <returns>SystemTypes of the parent or default if none is found</returns>
        public static SystemTypes GetParentOrDefault(LIElement element)
        {
            SystemTypes systemType = 0;
            if (element.properties.parent != null)
                systemType = GetSystem((Guid)element.properties.parent);
            return systemType;
        }
    }
}
