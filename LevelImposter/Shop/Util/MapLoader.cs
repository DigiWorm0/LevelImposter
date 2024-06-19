using LevelImposter.Core;
using System;

namespace LevelImposter.Shop
{
    public static class MapLoader
    {
        private static string? _lastMapID = null;
        private static LIMap? _currentMap = null;
        private static bool _isFallback = false;

        public static LIMap? CurrentMap => _currentMap;
        public static bool IsFallback => _isFallback;

        /// <summary>
        /// Loads a map from <c>LIMap</c> model
        /// </summary>
        /// <param name="map">LevelImposter map data</param>
        public static void LoadMap(LIMap? map, bool isFallback)
        {
            if (_lastMapID != map?.id)
                GCHandler.Clean();
            _lastMapID = map?.id;
            _currentMap = map;
            _isFallback = isFallback;

            if (map != null && LIShipStatus.Instance != null)
                LIShipStatus.Instance.LoadMap(map);

            // Lobby Message
            if (LobbyBehaviour.Instance != null)
                DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(StringNames.GameMapName, map?.name, false);
        }

        /// <summary>
        /// Loads a map based on ID from filesystem
        /// </summary>
        /// <param name="mapID">Map file name or ID without extension</param>
        /// <param name="callback">Callback on success</param>
        public static void LoadMap(string mapID, bool isFallback, Action? callback)
        {
            var mapData = MapFileAPI.Get(mapID);
            LoadMap(mapData, isFallback);
            callback?.Invoke(); // TODO: Make synchronous
        }

        /// <summary>
        /// Unloads any map, if loaded
        /// </summary>
        public static void UnloadMap()
        {
            _currentMap = null;
            _isFallback = false;
        }

        /// <summary>
        /// Manually sets the fallback flag on the loaded map
        /// </summary>
        /// <param name="isFallback">True iff the current map is fallback</param>
        public static void SetFallback(bool isFallback)
        {
            _isFallback = isFallback;
        }
    }
}
