using HarmonyLib;
using LevelImposter.Builders;

namespace LevelImposter.Core;

/// <summary>
///     Hides rooms from the RoomTracker that are hidden by the map.
///     This is done by temporarily removing the room collider.
///     I hate this, but I can't find a better way to do it.
/// </summary>
[HarmonyPatch(typeof(RoomTracker), nameof(RoomTracker.FixedUpdate))]
public static class RoomUIPatch
{
    public static void Prefix()
    {
        if (LIShipStatus.IsInstance())
            return;

        // Remove room colliders
        foreach (var roomData in RoomBuilder.RoomDB)
            if (!roomData.isUIVisible && roomData.shipRoom != null)
                roomData.shipRoom.roomArea = null;
    }

    public static void Postfix()
    {
        if (LIShipStatus.IsInstance())
            return;

        // Add room colliders
        foreach (var roomData in RoomBuilder.RoomDB)
            if (!roomData.isUIVisible && roomData.shipRoom != null)
                roomData.shipRoom.roomArea = roomData.collider;
    }
}