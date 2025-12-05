using System;
using LevelImposter.Core;

namespace LevelImposter.Shop;

public static class MapLoader
{
    private static string? _lastMapID;

    public static LIMap? CurrentMap { get; private set; }

    public static bool IsFallback { get; private set; }

    /// <summary>
    ///     Loads a map from <c>LIMap</c> model
    /// </summary>
    /// <param name="map">LevelImposter map data</param>
    public static void LoadMap(LIMap? map, bool isFallback)
    {
        // Wipe cache if map changed
        // (Keeps cache if replaying the same map)
        if (_lastMapID != map?.id)
            GCHandler.Clean();
        
        _lastMapID = map?.id;
        CurrentMap = map;
        IsFallback = isFallback;

        // Should never happen, but just in case - load the map
        if (map != null && LIShipStatus.IsInstance())
            LIShipStatus.GetInstance().Builder.BuildMap(map);

        // Only continue if in lobby
        if (!GameState.IsInLobby)
            return;

        // Lobby Message
        LobbyVersionTag.UpdateText();

        // Preload all sprites
        MapUtils.PreloadAllMapSprites();
        LoadingBar.Run();

        // Send map change message
        if (GameState.IsCustomMapSelected)
            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(
                StringNames.GameMapName,
                isFallback ? "Random" : map?.name,
                false
            );
    }

    /// <summary>
    ///     Loads a map based on ID from filesystem
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
    ///     Unloads any map, if loaded
    /// </summary>
    public static void UnloadMap()
    {
        CurrentMap = null;
        IsFallback = false;
    }

    /// <summary>
    ///     Manually sets the fallback flag on the loaded map
    /// </summary>
    /// <param name="isFallback">True iff the current map is fallback</param>
    public static void SetFallback(bool isFallback)
    {
        IsFallback = isFallback;

        // Lobby Message
        LobbyVersionTag.UpdateText();
    }
}