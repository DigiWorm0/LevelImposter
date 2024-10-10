using System;
using HarmonyLib;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Shop;

/*
 *      Replaces the map image with the map thumbnail
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class LobbyMapImagePatch
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
        var currentMap = MapLoader.CurrentMap;
        var isFallbackMap = MapLoader.IsFallback;
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
            _defaultThumbnail = MapUtils.LoadAssetBundle<Sprite>("defaultthumbnail");
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
        if (!isCustomMap || isFallbackMap)
            return;

        // Check if in cache
        if (ThumbnailCache.Exists(currentMap.id))
            ThumbnailCache.Get(currentMap.id, sprite =>
            {
                MapIcon.MapIcon = sprite;
                MapIcon.MapImage = sprite;
                MapIcon.NameImage = sprite;
            });
        // Download the thumbnail
        else if (!string.IsNullOrEmpty(currentMap?.thumbnailURL))
            LevelImposterAPI.DownloadThumbnail(currentMap, sprite =>
            {
                MapIcon.MapIcon = sprite;
                MapIcon.MapImage = sprite;
                MapIcon.NameImage = sprite;
            });
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
public static class LobbyTestPatch
{
    public static void Postfix(GameStartManager __instance)
    {
        LobbyMapImagePatch.CurrentMapID = null;
        LobbyMapImagePatch.MapIcon = new MapIconByName
        {
            Name = (MapNames)MapType.LevelImposter,
            MapIcon = null,
            MapImage = null,
            NameImage = null
        };
        __instance.AllMapIcons.Add(LobbyMapImagePatch.MapIcon);
    }
}