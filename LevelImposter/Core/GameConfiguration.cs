using AmongUs.GameOptions;
using LevelImposter.Lobby;
using LevelImposter.Shop;

namespace LevelImposter.Core;

public static class GameConfiguration
{
    /// <summary>
    /// The selected map type from the game options (Skeld, Mira, LevelImposter, etc.).
    /// </summary>
    public static MapType CurrentMapType => GameState.IsInFreeplay
        ? (MapType)AmongUsClient.Instance.TutorialMapId
        : (MapType)GameOptionsManager.Instance.CurrentGameOptions.MapId;
    
    /// <summary>
    /// Represents the currently selected lobby map data.
    /// </summary>
    public static LIMap? CurrentLobbyMap { get; private set; }
    
    /// <summary>
    /// Represents the currently active map data.
    /// </summary>
    public static LIMap? CurrentMap { get; private set; }
    
    /// <summary>
    /// If this is true, the map name is shown as "Random" in the lobby UI.
    /// Used if the current map is a randomized/fallback map.
    /// </summary>
    public static bool HideMapName { get; private set; }

    /// <summary>
    ///   Sets the currently active map to the provided map data.
    /// </summary>
    /// <param name="map">LevelImposter map data or null to clear the map</param>
    /// <param name="hideMapName">If true, the map name is shown as "Random" in the lobby UI</param>
    public static void SetMap(LIMap? map, bool hideMapName = false)
    {
        // Wipe cache if map changed
        // (Keeps cache if replaying the same map)
        if (CurrentMap?.id != map?.id)
            GCHandler.DisposeAll(GCBehavior.DisposeOnMapUnload);

        // Update current map
        CurrentMap = map;
        HideMapName = hideMapName;
        UpdateLobbyUI();
    }
    
    /// <summary>
    ///   Sets the currently selected lobby map data.
    /// </summary>
    /// <param name="map">LevelImposter map data or null to reset to dropship</param>
    public static void SetLobbyMap(LIMap? map)
    {
        // Wipe cache if map changed
        // (Keeps cache if replaying the same map)
        if (CurrentLobbyMap?.id != map?.id)
            GCHandler.DisposeAll(GCBehavior.DisposeOnLobbyUnload);
        
        // Update current lobby map
        CurrentLobbyMap = map;
    }

    /// <summary>
    /// Sets the currently selected map type in the game options.
    /// </summary>
    /// <param name="mapType">The map type to set</param>
    /// <param name="syncMapType">If true, syncs the map type to all clients</param>
    public static void SetMapType(MapType mapType, bool syncMapType = true)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        currentGameOptions.SetByte(ByteOptionNames.MapId, (byte)mapType);
        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
        if (syncMapType)
            GameManager.Instance.LogicOptions.SyncOptions();
    }

    /// <summary>
    /// Updates the lobby UI to reflect the current map state
    /// </summary>
    /// <param name="sendNotification">If true, sends a notification to the lobby about the map change</param>
    /// <param name="preloadSprites">If true, preloads all map sprites</param>
    private static void UpdateLobbyUI(
        bool sendNotification = true,
        bool preloadSprites = true)
    {
        // Check if we're in the lobby
        if (!GameState.IsInLobby)
            return;

        // Update version tag
        LobbyVersionTag.UpdateText();

        // Preload all sprites
        if (preloadSprites)
        {
            // TODO: FIX ME!
            MapUtils.PreloadAllMapSprites();
            LoadingBar.Run();
        }

        // Send map change message
        if (CurrentMap != null &&
            CurrentMapType == MapType.LevelImposter &&
            sendNotification)
        {
            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(
                StringNames.GameMapName,
                HideMapName ? "Random" : CurrentMap?.name,
                false
            );
        }
    }
}