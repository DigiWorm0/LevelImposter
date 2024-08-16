using HarmonyLib;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Replaces the map image with the map thumbnail
     */
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class LobbyMapImagePatch
    {
        private static readonly Vector3 _mapImagePos = new Vector3(-1.9224f, -2.5392f, -2.0f);
        private static readonly Vector3 _mapImageScale = new Vector3(1.6074f, 1.6074f, 1.6074f);

        private static string? _currentMapID = null;
        private static GameObject? _settingsHeader = null;

        public static MapIconByName? MapIcon = null;

        public static void Postfix(GameStartManager __instance)
        {
            // Get the current lobby UI state
            var currentMap = MapLoader.CurrentMap;
            bool isCustomMap = GameManager.Instance.LogicOptions.MapId == (byte)MapType.LevelImposter;
            string? mapID = isCustomMap ? currentMap?.id : null;
            bool isMapChanged = _currentMapID != mapID;

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

            // Check null variable
            if (MapIcon == null || currentMap == null)
                return;

            // Check if the map has changed
            if (!isMapChanged)
                return;
            _currentMapID = mapID;

            // TODO Set default map icon

            // Toggle Settings Header
            if (_settingsHeader == null)
                _settingsHeader = __instance.MapImage.transform.parent.FindChild("RoomSettingsHeader").gameObject;
            _settingsHeader.SetActive(!isCustomMap);

            // Load Thumbnail
            if (isCustomMap)
            {
                // Check if in cache
                if (ThumbnailCache.Exists(currentMap.id))
                {
                    ThumbnailCache.Get(currentMap.id, (sprite) =>
                    {
                        MapIcon.MapIcon = sprite;
                        MapIcon.MapImage = sprite;
                        MapIcon.NameImage = sprite;
                    });
                }
                // Download the thumbnail
                else if (!string.IsNullOrEmpty(currentMap?.thumbnailURL))
                {
                    LevelImposterAPI.DownloadThumbnail(currentMap, (sprite) =>
                    {
                        MapIcon.MapIcon = sprite;
                        MapIcon.MapImage = sprite;
                        MapIcon.NameImage = sprite;
                    });
                }
            }
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class LobbyTestPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            LobbyMapImagePatch.MapIcon = new MapIconByName()
            {
                Name = (MapNames)MapType.LevelImposter,
                MapIcon = null,
                MapImage = null,
                NameImage = null
            };
            __instance.AllMapIcons.Add(LobbyMapImagePatch.MapIcon);
        }
    }
}
