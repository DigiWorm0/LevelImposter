using HarmonyLib;
using Hazel;
using System.Linq;

namespace LevelImposter.Core
{
    /// <summary>
    /// Increases the maximum door ID from 31 to 63
    /// by using the unused 6th bit.
    /// </summary>
    [HarmonyPatch(typeof(DoorsSystemType), nameof(DoorsSystemType.UpdateSystem))]
    public static class DoorIDPatch
    {
        public static bool Prefix(DoorsSystemType __instance, [HarmonyArgument(1)] MessageReader msgReader)
        {
            if (LIShipStatus.Instance == null)
                return true;

            byte b = msgReader.ReadByte();
            int id = b & 63; // <-- This is the only change
            int num = b & 192;

            if (num != 64)
                return true;

            OpenableDoor openableDoor = ShipStatus.Instance.AllDoors.First((OpenableDoor d) => d.Id == id);
            openableDoor?.SetDoorway(true);
            if (openableDoor == null)
                LILogger.Warn($"Door ID {id} not found!");

            __instance.IsDirty = true;
            return false;
        }
    }
}