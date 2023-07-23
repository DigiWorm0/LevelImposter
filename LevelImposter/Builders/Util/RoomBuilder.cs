using LevelImposter.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class RoomBuilder : IElemBuilder
    {
        private static Dictionary<Guid, SystemTypes> _systemDB = new();
        private static Dictionary<SystemTypes, PlainShipRoom> _roomDB = new();

        public RoomBuilder()
        {
            _systemDB.Clear();
            _roomDB.Clear();
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-room")
                return;

            // ShipStatus
            if (LIShipStatus.Instance == null)
                throw new MissingShipException();

            // Pick a new System
            SystemTypes systemType = SystemDistributor.GetNewSystemType();

            // Plain Ship ROom
            PlainShipRoom shipRoom = obj.AddComponent<PlainShipRoom>();
            shipRoom.RoomId = systemType;
            shipRoom.roomArea = obj.GetComponentInChildren<Collider2D>();
            if (shipRoom.roomArea != null)
                shipRoom.roomArea.isTrigger = true;
            else if ((elem.properties.isRoomAdminVisible ?? true) || (elem.properties.isRoomNameVisible ?? true))
                LILogger.Warn($"{shipRoom.name} is missing a collider");

            // Rename Room Name
            LIShipStatus.Instance.Renames.Add(systemType, obj.name);

            // Add to DB
            _systemDB.Add(elem.id, systemType);
            _roomDB.Add(systemType, shipRoom);
        }

        public void PostBuild()
        {
            if (LIShipStatus.Instance == null)
                throw new MissingShipException();
            LIShipStatus.Instance.Renames.Add((SystemTypes)0, "Default Room");
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

        /// <summary>
        /// Gets the PlainShipRoom associated with a specific SystemTypes value.
        /// (<c>ShipStatus.FastRooms</c> is not yet loaded at the time)
        /// </summary>
        /// <param name="systemType">SystemTypes of the room</param>
        /// <returns>Associated PlainShipRoom component or <c>null</c> if none found</returns>
        public static PlainShipRoom? GetShipRoom(SystemTypes systemType)
        {
            return _roomDB.GetValueOrDefault(systemType);
        }
    }
}
