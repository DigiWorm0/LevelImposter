using System;
using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class RoomBuilder : IElemBuilder
{
    public RoomBuilder()
    {
        RoomDB.Clear();
    }

    public static List<RoomData> RoomDB { get; } = new();

    public void Build(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-room")
            return;

        // ShipStatus
        if (!LIShipStatus.IsInstance())
            throw new MissingShipException();

        // Pick a new System
        var systemType = SystemDistributor.GetNewSystemType();

        // Options
        var isAdminVisible = elem.properties.isRoomAdminVisible ?? true;
        var isUIVisible = elem.properties.isRoomUIVisible ?? true;

        // Create ShipRoom
        var shipRoom = obj.AddComponent<PlainShipRoom>();
        shipRoom.RoomId = systemType;
        shipRoom.roomArea = obj.GetComponentInChildren<Collider2D>();

        // Fix Collider
        if (shipRoom.roomArea != null)
            shipRoom.roomArea.isTrigger = true;
        else if (isAdminVisible || isUIVisible)
            LILogger.Warn($"{shipRoom.name} is missing a collider");

        // Rename Room Name
        LIShipStatus.GetInstanceOrNull()?.Renames.Add(systemType, obj.name);

        // Add to DB
        RoomDB.Add(new RoomData
        {
            elementID = elem.id,
            isUIVisible = isUIVisible,
            shipRoom = shipRoom,
            collider = shipRoom.roomArea
        });
    }

    public void PostBuild()
    {
        if (!LIShipStatus.IsInstance())
            throw new MissingShipException();

        // Add Default Room Name
        LIShipStatus.GetInstanceOrNull()?.Renames.Add((SystemTypes)0, "Default Room");
    }

    /// <summary>
    ///     Gets the SystemTypes associated with a specific util-room ID
    /// </summary>
    /// <param name="id">ID of the util-room object</param>
    /// <returns>Associated SystemTypes value</returns>
    public static SystemTypes GetSystem(Guid id)
    {
        return RoomDB.FirstOrDefault(x => x.elementID == id).systemType ?? 0;
    }

    /// <summary>
    ///     Gets the SystemTypes of the parent of an object
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
    ///     Gets the PlainShipRoom associated with a specific SystemTypes value.
    ///     (<c>ShipStatus.FastRooms</c> is not yet loaded at the time)
    /// </summary>
    /// <param name="systemType">SystemTypes of the room</param>
    /// <returns>Associated PlainShipRoom component or <c>null</c> if none found</returns>
    public static PlainShipRoom? GetShipRoom(SystemTypes systemType)
    {
        return RoomDB.FirstOrDefault(x => x.systemType == systemType).shipRoom;
    }

    /// <summary>
    ///     Gets whether the UI is visible for a specific SystemTypes value.
    /// </summary>
    /// <param name="systemType">Room ID to check</param>
    /// <returns>True if the UI is visible, false otherwise</returns>
    public static bool GetUIVisible(SystemTypes systemType)
    {
        return RoomDB.FirstOrDefault(x => x.systemType == systemType).isUIVisible;
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