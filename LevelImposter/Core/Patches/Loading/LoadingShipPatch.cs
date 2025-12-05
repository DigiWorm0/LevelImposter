using HarmonyLib;
using InnerNet;

namespace LevelImposter.Core;

/// <summary>
///     Waits for the ship to finish loading before sending the client ready packet.
/// </summary>
[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SendClientReady))]
public static class LoadingShipPatch
{
    public static bool Prefix(InnerNetClient __instance)
    {
        // // Continue if not in a game
        // if (!LIShipStatus.IsInstance())
        //     return true;
        //
        // // Continue if ship is already loaded
        // if (LIShipStatus.GetInstance().IsReady)
        //     return true;
        //
        // // Wait for ship to finish loading, then send packet
        // MapUtils.WaitForShip(LIConstants.MAX_LOAD_TIME, __instance.SendClientReady);
        //
        // // Don't send packet
        // return false;
        // TODO: FIX ME
        return true;
    }
}