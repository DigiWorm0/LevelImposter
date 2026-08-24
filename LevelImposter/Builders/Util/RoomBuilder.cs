using System;
using System.Collections.Generic;
using System.Linq;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.Models;
using LevelImposter.Core.Services;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class RoomBuilder
{
    public static List<RoomData> RoomDB { get; } = [];

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        RoomDB.Clear();
    }

    [ElementBuilder(
        Priority = Priority.VERY_HIGH,
        ElementTypes = ["util-room"]
    )]
    public static void Build(LIElement element, GameObject gameObject)
    {
        // Pick a new System
        var systemType = SystemDistributionService.GetNewSystemType();

        // Options
        var isAdminVisible = element.properties.isRoomAdminVisible ?? true;
        var isUIVisible = element.properties.isRoomUIVisible ?? true;

        // Create ShipRoom
        var shipRoom = gameObject.AddComponent<PlainShipRoom>();
        shipRoom.RoomId = systemType;
        shipRoom.roomArea = gameObject.GetComponentInChildren<Collider2D>();

        // Fix Collider
        if (shipRoom.roomArea != null)
            shipRoom.roomArea.isTrigger = true;
        else if (isAdminVisible || isUIVisible)
            LILogger.Warn($"{shipRoom.name} is missing a collider");

        // Rename Room Name
        LIBaseShip.Instance?.Renames.Add(systemType, gameObject.name);

        // Add to DB
        RoomDB.Add(new RoomData
        {
            ElementID = element.id,
            IsUIVisible = isUIVisible,
            ShipRoom = shipRoom,
            Collider = shipRoom.roomArea
        });

        // TODO: Add shiproom to lobby behaviour
    }

    [MapBuilder]
    public static void AddDefaultRoom()
    {
        LIBaseShip.Instance?.Renames.Add((SystemTypes)0, "Default Room");
    }

    /// <summary>
    ///     Gets the SystemTypes associated with a specific util-room ID
    /// </summary>
    /// <param name="id">ID of the util-room object</param>
    /// <returns>Associated SystemTypes value</returns>
    public static SystemTypes GetSystem(Guid id)
    {
        return RoomDB.FirstOrDefault(x => x.ElementID == id).SystemType ?? 0;
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
        return RoomDB.FirstOrDefault(x => x.SystemType == systemType).ShipRoom;
    }

    public readonly struct RoomData
    {
        public Guid ElementID { get; init; }
        public bool IsUIVisible { get; init; }
        public PlainShipRoom? ShipRoom { get; init; }
        public Collider2D? Collider { get; init; }
        public SystemTypes? SystemType => ShipRoom?.RoomId;
    }
}