using HarmonyLib;

namespace LevelImposter.Core;

/// <summary>
///     Fixes a bug when PoolablePlayer runs Awake
///     too early while meeting prefabs are being instantiated.
/// </summary>
[HarmonyPatch(typeof(MeetingCalledAnimation), nameof(MeetingCalledAnimation.Initialize))]
public static class MeetingOverlayPatch
{
    public static void Prefix(MeetingCalledAnimation __instance)
    {
        if (!LIShipStatus.IsInstance())
            return;

        // Re-initialize player parts
        __instance.playerParts.InitBody();
    }
}