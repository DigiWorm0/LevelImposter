using System.Linq;
using HarmonyLib;
using Hazel;

namespace LevelImposter.Core;

/// <summary>
///     Increases the maximum door ID from 31 to 63
///     by using the unused 6th bit.
/// </summary>
[HarmonyPatch(typeof(DoorsSystemType), nameof(DoorsSystemType.UpdateSystem))]
public static class DoorIDPatch
{
    public static bool Prefix(DoorsSystemType __instance, [HarmonyArgument(1)] MessageReader msgReader)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        var b = msgReader.ReadByte();
        var id = b & 63; // <-- This is the only change
        var num = b & 192;

        if (num != 64)
            return true;

        var openableDoor = ShipStatus.Instance.AllDoors.First(d => d.Id == id);
        openableDoor?.SetDoorway(true);
        if (openableDoor == null)
            LILogger.Warn($"Door ID {id} not found!");

        __instance.IsDirty = true;
        return false;
    }
}