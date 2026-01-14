using HarmonyLib;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Runs a variety of initialization tasks after the game has started.
/// </summary>
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
public static class LateInitPatch
{
    private static bool _hasInitialized;

    public static void Postfix()
    {
        if (_hasInitialized)
            return;

        // Add Mod Stamp (In case Reactor is missing)
        DestroyableSingleton<ModManager>.Instance.ShowModStamp();

        // Increase player's max movement range
        // from (-50 - 50) to (-500 - 500)
        NetHelpers.XRange = new FloatRange(-500f, 500f);
        NetHelpers.YRange = new FloatRange(-500f, 500f);

        // Increase SystemTypes range to fix
        // ShipStatus.Deteriorate and ShipStatus.Serialize
        var allTypes = new SystemTypes[byte.MaxValue];
        for (var i = 0; i < byte.MaxValue; i++)
            allTypes[i] = (SystemTypes)i;
        SystemTypeHelpers.AllTypes = allTypes;

        // Add Global Components that utilize
        // the Unity runtime in some way
        var apiParent = new GameObject("LevelImposter");
        apiParent.AddComponent<LagLimiter>();
        Object.DontDestroyOnLoad(apiParent);

        _hasInitialized = true;
    }
}