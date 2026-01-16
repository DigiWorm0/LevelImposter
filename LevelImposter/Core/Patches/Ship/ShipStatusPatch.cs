using HarmonyLib;
using LevelImposter.Shop;

namespace LevelImposter.Core;

/// <summary>
///     Appends the <c>LIShipStatus</c> component to LevelImposter maps.
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
public static class ShipStatusPatch
{
    public static void Prefix(ShipStatus __instance)
    {
        //UnityToMapGenerator.GenerateMap(__instance);

        if (GameConfiguration.CurrentMapType == MapType.LevelImposter)
            __instance.gameObject.AddComponent<LIShipStatus>();
        else if (!GameConfiguration.HideMapName)
            LILogger.Msg("Another mod has changed the current map state");
    }
}