using HarmonyLib;
using InnerNet;
using LevelImposter.Core;
using LevelImposter.Shop;

namespace LevelImposter.Lobby;

public static class PreventGameStartHelper
{
    /// <summary>
    /// Checks if the game can start
    /// </summary>
    /// <param name="reason">The reason why the game can't start</param>
    /// <returns>TRUE if the game can start, FALSE otherwise</returns>
    public static bool CanStartGame(out string reason)
    {
        // Check is local player had errors downloading the map
        var mapDownloadState = GameConfigurationSync.GameMapDownloader.CurrentDownloadState;
        if (mapDownloadState?.Error != null) {
            reason = $"ERROR: {mapDownloadState?.Error}";
            return false;
        }

        // Check if local player is still downloading
        if (mapDownloadState != null) {
            reason = $"DOWNLOADING MAP ({mapDownloadState.Progress * 100:F1}%)";
            return false;
        }

        
        // Check if all players have downloaded the map
        var notReadyPlayerCount = PlayersReadyCounter.NotReadyPlayers.Count;
        if (notReadyPlayerCount > 1)
        {
            reason = $"WAITING ON <color=#1a95d8>{notReadyPlayerCount} players</color> TO DOWNLOAD MAP";
            return false;
        }
        if (notReadyPlayerCount == 1)
        {
            var notReadyPlayer = PlayersReadyCounter.NotReadyPlayers[0];
            reason = $"WAITING ON <color=#1a95d8>{notReadyPlayer.name}</color> TO DOWNLOAD MAP";
            return false;
        }
        
        // Check if there is a map available
        if (GameConfiguration.CurrentMapType == MapType.LevelImposter &&
            GameConfiguration.CurrentMap == null)
        {
            reason = "NO MAPS AVAILABLE";
            return false;
        }
        
        // All checks passed
        reason = string.Empty;
        return true;
    }
}

/**
 *      Disables the Start Button if
 *      not all players have a downloaded map.
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
public static class LobbyGameStartPatch
{
    public static bool Prefix()
    {
        return PreventGameStartHelper.CanStartGame(out _);
    }
}

/**
 *      Updates the start button text
 *      on the bottom of the lobby
 */
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class LobbyButtonTextPatch
{
    private static bool _isDownloadTextDisplayed;
    
    public static void Postfix(GameStartManager __instance)
    {
        var canStartGame = PreventGameStartHelper.CanStartGame(out var buttonText);
        
        // Check if a player is downloading
        if (!canStartGame)
        {
            // Disable Button
            __instance.StartButton.SetButtonEnableState(false);
            
            // Set Text
            __instance.GameStartTextClient.text = buttonText; // Client
            __instance.StartButton.ChangeButtonText(buttonText); // Host

            // Set flag
            _isDownloadTextDisplayed = true;
        }
        
        // Check if we need to clear the text
        else if (_isDownloadTextDisplayed)
        {
            // Force GameStartManager.Update to update the button state
            __instance.LastPlayerCount = -1;

            // Clear Text
            __instance.GameStartTextClient.text = string.Empty;

            // Clear flag
            _isDownloadTextDisplayed = false;
        }
    }
}