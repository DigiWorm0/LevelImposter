using System;
using HarmonyLib;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.Cache;
using UnityEngine;

namespace LevelImposter.Lobby.Patches;

/*
 *      Applies the thumbnail image on the right lobby window.
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class LobbyThumbnailPatch
{
    private static readonly Vector3 MapImagePos = new(-2.0f, -2.25f, -2.2f);
    private static readonly Vector3 MapImageScale = new(0.91f, 0.91f, 1.0f);

    private static SpriteRenderer? _thumbnailRenderer;
    private static SpriteRenderer? _backgroundRenderer;
    private static string? _activeThumbnailID;
    private static Sprite? _defaultThumbnail;

    public static void Postfix(GameStartManager __instance)
    {
        // If the default thumbnail is null, load it
        if (_defaultThumbnail == null)
            _defaultThumbnail = PackagedResources.LoadFromBundle<Sprite>("DefaultThumbnail");
        if (_defaultThumbnail == null)
            throw new Exception("Error loading default thumbnail from asset bundle");

        // If the thumbnail renderer is null, create it
        if (_thumbnailRenderer == null)
        {
            var thumbnailRendererObj = new GameObject("LI_MapThumbnailRenderer");
            thumbnailRendererObj.transform.SetParent(__instance.MapImage?.transform.parent);
            thumbnailRendererObj.transform.localPosition = MapImagePos;
            thumbnailRendererObj.transform.localScale = MapImageScale;
            thumbnailRendererObj.layer = (int)Layer.UI;

            _thumbnailRenderer = thumbnailRendererObj.AddComponent<SpriteRenderer>();
            _thumbnailRenderer.sprite = _defaultThumbnail;

            // Force Refresh Thumbnail
            _activeThumbnailID = null;

            // TOU-Mira Compatibility
            if (ModCompatibility.IsLobbyUICompatibilityEnabled)
            {
                thumbnailRendererObj.transform.localPosition = new Vector3(-2.0f, -2.55f, -3.2f);
                thumbnailRendererObj.transform.localScale = new Vector3(0.75f, 0.75f, 1.0f);

                var backgroundObject = new GameObject("Background");
                backgroundObject.layer = (int)Layer.UI;
                backgroundObject.transform.parent = thumbnailRendererObj.transform;
                backgroundObject.transform.localPosition = new Vector3(0, 0, 0.1f);
                backgroundObject.transform.localScale = new Vector3(1.27f, 1.0f, 1.0f);

                _backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
                _backgroundRenderer.sprite = _defaultThumbnail;
                _backgroundRenderer.color = new Color(0.15f, 0.15f, 0.15f, 1.0f);
            }
        }

        // Update thumbnail visibility
        var isThumbnailEnabled = GameConfiguration.CurrentMapType == MapType.LevelImposter;
        _thumbnailRenderer.gameObject.SetActive(isThumbnailEnabled);
        if (__instance.MapImage != null)
            __instance.MapImage.enabled = !isThumbnailEnabled;

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
        if (_backgroundRenderer != null)
            _backgroundRenderer.sprite = _defaultThumbnail;
        if (_activeThumbnailID != null &&
            !GameConfiguration.HideMapName &&
            GameConfiguration.CurrentMap != null &&
            GameConfiguration.CurrentMap.HasThumbnail)
            ThumbnailCache.Get(_activeThumbnailID, UpdateMapThumbnail);
    }

    private static void UpdateMapThumbnail(Sprite? sprite)
    {
        if (sprite == null)
            return;

        if (_thumbnailRenderer != null)
            _thumbnailRenderer.sprite = sprite;
        if (_backgroundRenderer != null)
            _backgroundRenderer.sprite = sprite;
    }
}