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
        __result = GameState.MapName;
        return false;
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
// TODO: Fix thumbnails
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class MapIconPatch
{
    private static readonly Vector3 MapImagePos = new(-2.0f, -2.25f, -2.2f);
    private static readonly Vector3 MapImageScale = new(0.91f, 0.91f, 1.0f);

    private static SpriteRenderer? _thumbnailRenderer;
    private static string? _activeThumbnailID;
    private static GameObject? _settingsHeader;
    private static Sprite? _defaultThumbnail;

    public static void Postfix(GameStartManager __instance)
    {
        // If the default thumbnail is null, load it
        if (_defaultThumbnail == null)
            _defaultThumbnail = MapUtils.LoadResourceFromAssetBundle<Sprite>("defaultthumbnail");
        if (_defaultThumbnail == null)
            throw new Exception("Error loading default thumbnail from asset bundle");
        
        // If the thumbnail renderer is null, create it
        if (_thumbnailRenderer == null)
        {
            var thumbnailRendererObj = new GameObject("LI_MapThumbnailRenderer");
            thumbnailRendererObj.transform.SetParent(__instance.MapImage.transform.parent);
            thumbnailRendererObj.transform.localPosition = MapImagePos;
            thumbnailRendererObj.transform.localScale = MapImageScale;
            thumbnailRendererObj.layer = (int)Layer.UI;
            
            _thumbnailRenderer = thumbnailRendererObj.AddComponent<SpriteRenderer>();
            _thumbnailRenderer.sprite = _defaultThumbnail;
        }

        // Update thumbnail visibility
        _thumbnailRenderer.enabled = GameConfiguration.CurrentMapType == MapType.LevelImposter;
        
        // Get Map ID
        var currentMapID = GameConfiguration.CurrentMap?.id;
        if (GameConfiguration.HideMapName ||
            GameConfiguration.CurrentMapType != MapType.LevelImposter)
            currentMapID = null;

        // Check if the thumbnail has changed
        if (currentMapID == _activeThumbnailID)
            return;
        _activeThumbnailID = currentMapID;

        // Reload Thumbnail
        _thumbnailRenderer.sprite = _defaultThumbnail;
        if (_activeThumbnailID != null &&
            !GameConfiguration.HideMapName &&
            GameConfiguration.CurrentMap != null &&
            GameConfiguration.CurrentMap.HasThumbnail)
        {
            ThumbnailCache.Get(_activeThumbnailID, UpdateMapThumbnail);
        }
    }
    
    private static void UpdateMapThumbnail(Sprite? sprite)
    {
        if (_thumbnailRenderer == null || sprite == null)
            return;
        
        _thumbnailRenderer.sprite = sprite;
    }
}