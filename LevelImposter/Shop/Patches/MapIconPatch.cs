using System;
using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Networking.API;
using UnityEngine;

namespace LevelImposter.Shop;

/*
 *      Creates and applies a custom map icon/button for the LevelImposter map in the map picker and lobby.
 */
[HarmonyPatch(typeof(CreateGameMapPicker), nameof(CreateGameMapPicker.Initialize))]
public static class CreateLobbyMapIconPatch
{
    public static void Prefix(CreateGameMapPicker __instance)
    {
        __instance.AllMapIcons.Add(LIMapIconBuilder.Get());
    }
}
[HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.Initialize))]
public static class LobbySettingMapIconPatch
{
    private const float ICON_OFFSET_X = -0.1f;
    
    public static void Prefix(GameOptionsMapPicker __instance)
    {
        __instance.AllMapIcons.Add(LIMapIconBuilder.Get());
        
        // Move icons to the left slightly to accommodate
        __instance.Labeltext.transform.parent.localPosition += new Vector3(ICON_OFFSET_X, 0.0f, 0.0f);
        __instance.StartPosX += ICON_OFFSET_X;
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class GameInfoMapIconPatch
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.AllMapIcons.Add(LIMapIconBuilder.Get());
    }
}
[HarmonyPatch(typeof(MapSelectionGameSetting), nameof(MapSelectionGameSetting.GetValueString))]
public static class MapSettingValueStringPatch
{
    public static bool Prefix([HarmonyArgument(0)] float value, ref string __result)
    {
        if ((int)value != (int)MapType.LevelImposter - 1) // <-- MapSelectionGameSetting.TryGetInt subtracts 1 for some reason
            return true;
        __result = TranslationController.Instance.GetString(LIConstants.MAP_STRING_NAME);
        return false;
    }
}
[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
public static class MapTooltipPatch
{
    public static void Postfix(CreateGameOptions __instance)
    {
        // Add tooltip
        while (__instance.mapTooltips.Length <= (int)MapType.LevelImposter)
            __instance.mapTooltips = MapUtils.AddToArr(__instance.mapTooltips, LIConstants.MAP_STRING_NAME);
        
        // Add wordart banner
        var wordart = LIMapIconBuilder.Get().NameImage;
        while (__instance.mapBanners.Length <= (int)MapType.LevelImposter)
            __instance.mapBanners = MapUtils.AddToArr(__instance.mapBanners, wordart); // IMAGE-SkeldBanner-Wordart (342x72)
        
        // Add crewmate background
        var background = MapUtils.LoadSpriteResource("LOBBY-Background.png");
        while (__instance.bgCrewmates.Length <= (int)MapType.LevelImposter)
            __instance.bgCrewmates = MapUtils.AddToArr(__instance.bgCrewmates, background); // BACKGROUND-4 players (515x895)
    }
}
[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.SetCrewmateGraphic))]
public static class MapChangedCrewmatePatch
{
    public static bool Prefix(CreateGameOptions __instance)
    {
        return __instance.mapPicker.GetSelectedID() != (int)MapType.LevelImposter;
    }
}