using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Components;

namespace LevelImposter.Lobby.Utils;

public static class LobbyUIService
{
    /// <summary>
    ///     Initializes global events
    /// </summary>
    public static void Init()
    {
        GameConfiguration.OnMapChange += UpdateLobbyUI;
    }

    /// <summary>
    ///     Updates the lobby UI to reflect the current map state
    /// </summary>
    /// <param name="sendNotification">If true, sends a notification to the lobby about the map change</param>
    /// <param name="preloadSprites">If true, preloads all map sprites</param>
    private static void UpdateLobbyUI()
    {
        // Check if we're in the lobby
        if (!GameState.IsInLobby)
            return;

        // Update version tag
        LobbyVersionTag.UpdateText();

        // Send map change message
        if (GameConfiguration.CurrentMap != null &&
            GameConfiguration.CurrentMapType == MapType.LevelImposter)
            DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(
                StringNames.GameMapName,
                GameConfiguration.HideMapName ? "Random" : GameConfiguration.CurrentMap.name,
                false
            );
    }
}