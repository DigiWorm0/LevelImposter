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
        private static readonly Vector3 _mapImagePos = new Vector3(-1.9f, -2.5f, -2.0f);
        private static readonly Vector3 _mapImageScale = Vector3.one;

        private static string? _currentMapID = null;

        public static MapIconByName? MapIcon = null;

        public static void Postfix(GameStartManager __instance)
        {
            // Adjust Transform
            __instance.MapImage.transform.localScale = _mapImageScale * 0.85f;
            __instance.MapImage.transform.localPosition = _mapImagePos + new Vector3(-0.1f, 0.1f, 0.0f);

            if (MapIcon == null)
                return;
            if (MapLoader.CurrentMap == null)
                return;
            if (GameManager.Instance.LogicOptions.MapId != (byte)MapType.LevelImposter)
                return;

            // TODO: Show generic thumbnail 

            var currentMap = MapLoader.CurrentMap;
            if (_currentMapID == currentMap.id)
                return;
            if (string.IsNullOrEmpty(currentMap?.thumbnailURL))
                return;

            _currentMapID = currentMap.id;

            if (ThumbnailCache.Exists(currentMap.id))
            {
                ThumbnailCache.Get(currentMap.id, (sprite) =>
                {
                    MapIcon.MapIcon = sprite;
                    MapIcon.MapImage = sprite;
                    MapIcon.NameImage = sprite;
                });
            }
            else
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
