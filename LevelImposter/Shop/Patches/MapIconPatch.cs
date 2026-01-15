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
public static class MapIconInitPatch
{
    public static void Prefix(CreateGameMapPicker __instance)
    {
        MapIconPatch.CurrentMapID = null;
        MapIconPatch.MapIcon = new MapIconByName
        {
            Name = (MapNames)MapType.LevelImposter,
            MapIcon = null, // ICON-Skeld (78x78)
            MapImage = null, // IMAGE-SkeldBanner (844x89)
            NameImage = null // IMAGE-SkeldBannerWordart (342x72)
        };
        __instance.AllMapIcons.Add(MapIconPatch.MapIcon);
    }
}
[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
public static class MapTooltipPatch
{
    public static void Postfix(CreateGameOptions __instance)
    {
        // Add tooltip for LevelImposter map
        while (__instance.mapTooltips.Length <= (int)MapType.LevelImposter)
            __instance.mapTooltips = MapUtils.AddToArr(__instance.mapTooltips, LIConstants.MAP_STRING_NAME);
        
        // Add banner for LevelImposter map
        while (__instance.mapBanners.Length <= (int)MapType.LevelImposter)
            __instance.mapBanners = MapUtils.AddToArr(__instance.mapBanners, null); // IMAGE-SkeldBanner-Wordart (342x72)
        
        while (__instance.bgCrewmates.Length <= (int)MapType.LevelImposter)
            __instance.bgCrewmates = MapUtils.AddToArr(__instance.bgCrewmates, null); // BACKGROUND-4 players (515x895)
    }
}
[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.SetCrewmateGraphic))]
public static class MapChangedCrewmatePatch
{
    public static void Prefix(CreateGameOptions __instance)
    {
        if (__instance.mapPicker.GetSelectedID() != (int)MapType.LevelImposter)
            return;
        
        __instance.currentCrewSprites = __instance.skeldCrewSprites;
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class MapIconPatch
{
    private static readonly Vector3 _mapImagePos = new(-1.9224f, -2.5392f, -2.0f);
    private static readonly Vector3 _mapImageScale = new(1.6074f, 1.6074f, 1.6074f);

    private static GameObject? _settingsHeader;
    private static Sprite? _defaultThumbnail;

    public static string? CurrentMapID;
    public static MapIconByName? MapIcon;

    public static void Postfix(GameStartManager __instance)
    {
        // Null Check
        if (__instance.MapImage == null)
            return;

        // Get the current lobby UI state
        var currentMap = GameConfiguration.CurrentMap;
        var isFallbackMap = GameConfiguration.HideMapName;
        var isCustomMap = GameManager.Instance?.LogicOptions.MapId == (byte)MapType.LevelImposter;

        // Get Map ID
        var mapID = currentMap?.id;
        if (isFallbackMap)
            mapID = "Fallback";
        if (!isCustomMap)
            mapID = null;

        // Update the scale
        if (isCustomMap)
        {
            __instance.MapImage.transform.localScale = _mapImageScale * 0.59f;
            __instance.MapImage.transform.localPosition = _mapImagePos + new Vector3(-0.08f, 0.25f, -1.0f);
        }
        else
        {
            __instance.MapImage.transform.localScale = _mapImageScale;
            __instance.MapImage.transform.localPosition = _mapImagePos;
        }

        // Null Check
        if (MapIcon == null || currentMap == null)
            return;

        // Check if the map has changed
        if (CurrentMapID == mapID)
            return;
        CurrentMapID = mapID;

        // Get default map icon
        if (_defaultThumbnail == null)
            _defaultThumbnail = MapUtils.LoadResourceFromAssetBundle<Sprite>("defaultthumbnail");
        if (_defaultThumbnail == null)
            throw new Exception("Error loading default thumbnail from asset bundle");

        // Set the default map icon
        MapIcon.MapIcon = _defaultThumbnail;
        MapIcon.MapImage = _defaultThumbnail;
        MapIcon.NameImage = _defaultThumbnail;

        // Toggle Settings Header
        if (_settingsHeader == null)
            _settingsHeader = __instance.MapImage.transform.parent.FindChild("RoomSettingsHeader").gameObject;
        _settingsHeader.SetActive(!isCustomMap);

        // Load Thumbnail
        if (isCustomMap &&
            !isFallbackMap &&
            currentMap.HasThumbnail)
        {
            ThumbnailCache.Get(mapID ?? "", SetMapIcon);
        }
    }

    private static void SetMapIcon(Sprite sprite)
    {
        if (MapIcon == null)
            return;
        MapIcon.MapIcon = sprite;
        MapIcon.MapImage = sprite;
        MapIcon.NameImage = sprite;
    }
}