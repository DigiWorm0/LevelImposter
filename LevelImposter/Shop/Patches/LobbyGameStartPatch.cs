using HarmonyLib;
using InnerNet;
using LevelImposter.Core;

namespace LevelImposter.Shop;

public static class PreventGameStartHelper
{
    /// <summary>
    /// Checks if the game can start
    /// </summary>
    /// <param name="reason">The reason why the game can't start</param>
    /// <returns>TRUE if the game can start, FALSE otherwise</returns>
    public static bool CanStartGame(out string reason)
    {
        // Check if all players have downloaded the map
        if (!DownloadManager.CanStart)
        {
            reason = DownloadManager.GetStartText();
            return false;
        }
        
        // Check if there is a map available
        if (GameConfiguration.CurrentMapType == MapType.LevelImposter && GameConfiguration.CurrentMap == null)
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

/*
 *      Remove player from DownloadManager
 *      if the player disconnects
 */
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
public static class DisconnectPatch
{
    public static void Postfix([HarmonyArgument(0)] ClientData data)
    {
        if (data.Character != null)
            DownloadManager.RemovePlayer(data.Character);
    }
}