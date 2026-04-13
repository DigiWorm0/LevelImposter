using HarmonyLib;
using Il2CppSystem;
using LevelImposter.Shop;
using ObjList = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>;

namespace LevelImposter.Core;

/// <summary>
///     Renames task names and other strings stored as <c>SystemTypes</c>.
/// </summary>
[HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(SystemTypes))]
public static class SystemRenamePatch
{
    public static bool Prefix([HarmonyArgument(0)] SystemTypes systemType, ref string __result)
    {
        if (!LIBaseShip.Instance?.Renames.Contains(systemType) ?? true)
            return true;

        __result = LIBaseShip.Instance.Renames.Get(systemType) ?? __result;
        return false;
    }
}

/// <summary>
///     Renames task names and other strings stored as <c>TaskTypes</c>.
/// </summary>
[HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(TaskTypes))]
public static class TaskRenamePatch
{
    public static bool Prefix([HarmonyArgument(0)] TaskTypes taskType, ref string __result)
    {
        if (!LIBaseShip.Instance?.Renames.Contains(taskType) ?? true)
            return true;

        __result = LIBaseShip.Instance.Renames.Get(taskType) ?? __result;
        return false;
    }
}

/// <summary>
///     Renames task names and other strings stored as <c>StringNames</c>.
/// </summary>
[HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames),
    typeof(ObjList))]
public static class StringRenamePatch
{
    public static bool Prefix([HarmonyArgument(0)] StringNames stringNames,
        [HarmonyArgument(1)] ObjList objList,
        ref string __result)
    {
        // Handle Special Cases
        if (stringNames == LIConstants.MAP_STRING_NAME)
        {
            // Hide map name in lobby
            if (GameConfiguration.HideMapName && GameState.IsInLobby)
                __result = LIConstants.MAP_NAME;
            
            // Otherwise, get the current map name
            else
                __result = GameConfiguration.CurrentMap?.name ?? LIConstants.MAP_NAME;
            return false;
        }

        // Handle Normal Cases
        if (!LIBaseShip.Instance?.Renames.Contains(stringNames) ?? true)
            return true;

        // Get the string from the database
        __result = LIBaseShip.Instance.Renames.Get(stringNames) ?? __result;

        // Format parameters into string
        // MUST BE Il2cppSystem.String.Format (not System.String.Format)
        __result = String.Format(__result, objList);

        return false;
    }
}