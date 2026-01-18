using System;
using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.FileIO;
using UnityEngine;

namespace LevelImposter.Lobby;

/*
 *      Applies the thumbnail image on the right lobby window.
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class LobbyThumbnailPatch
{
    private static readonly Vector3 MapImagePos = new(-2.0f, -2.25f, -2.2f);
    private static readonly Vector3 MapImageScale = new(0.91f, 0.91f, 1.0f);

    private static SpriteRenderer? _thumbnailRenderer;
    private static string? _activeThumbnailID;
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
        __instance.MapImage.enabled = GameConfiguration.CurrentMapType != MapType.LevelImposter;
        
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