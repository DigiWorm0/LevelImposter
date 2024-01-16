using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelImposter.Builders
{
    public class RoomBuilder : IElemBuilder
    {
        private static List<RoomData> _roomDB = new();
        public static List<RoomData> RoomDB => _roomDB;

        public RoomBuilder()
        {
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

            // Options
            bool isAdminVisible = elem.properties.isRoomAdminVisible ?? true;
            bool isUIVisible = elem.properties.isRoomUIVisible ?? true;

            // Create ShipRoom
            PlainShipRoom shipRoom = obj.AddComponent<PlainShipRoom>();
            shipRoom.RoomId = systemType;
            shipRoom.roomArea = obj.GetComponentInChildren<Collider2D>();

            // Fix Collider
            if (shipRoom.roomArea != null)
                shipRoom.roomArea.isTrigger = true;
            else if (isAdminVisible || isUIVisible)
                LILogger.Warn($"{shipRoom.name} is missing a collider");

            // Rename Room Name
            LIShipStatus.Instance.Renames.Add(systemType, obj.name);

            // Add to DB
            _roomDB.Add(new()
            {
                elementID = elem.id,
                isUIVisible = isUIVisible,
                shipRoom = shipRoom,
                collider = shipRoom.roomArea,
            });
        }

        public void PostBuild()
        {
            if (LIShipStatus.Instance == null)
                throw new MissingShipException();

            // Add Default Room Name
            LIShipStatus.Instance.Renames.Add((SystemTypes)0, "Default Room");
        }

        /// <summary>
        /// Gets the SystemTypes associated with a specific util-room ID
        /// </summary>
        /// <param name="id">ID of the util-room object</param>
        /// <returns>Associated SystemTypes value</returns>
        public static SystemTypes GetSystem(Guid id)
        {
            return _roomDB.FirstOrDefault(x => x.elementID == id).systemType ?? 0;
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
            return _roomDB.FirstOrDefault(x => x.systemType == systemType).shipRoom;
        }

        /// <summary>
        /// Gets whether the UI is visible for a specific SystemTypes value.
        /// </summary>
        /// <param name="systemType">Room ID to check</param>
        /// <returns>True if the UI is visible, false otherwise</returns>
        public static bool GetUIVisible(SystemTypes systemType)
        {
            return _roomDB.FirstOrDefault(x => x.systemType == systemType).isUIVisible;
        }

        public struct RoomData
        {
            public Guid elementID { get; set; }
            public bool isUIVisible { get; set; }
            public PlainShipRoom? shipRoom { get; set; }
            public Collider2D? collider { get; set; }
            public SystemTypes? systemType => shipRoom?.RoomId;
        }
    }
}
