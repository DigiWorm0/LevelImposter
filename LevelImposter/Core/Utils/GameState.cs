using LevelImposter.Shop;
using UnityEngine.SceneManagement;

namespace LevelImposter.Core;

public static class GameState
{
    public static bool IsHost => AmongUsClient.Instance?.AmHost ?? false;

    public static bool IsInFreeplay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInLobby => LobbyBehaviour.Instance != null;
    public static bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";
    public static bool IsInShop => ShopManager.Instance != null;
    public static bool IsInMeeting => MeetingHud.Instance != null;

    public static bool IsInCustomMap => LIShipStatus.GetInstanceOrNull() != null;
    public static bool IsCustomMapLoaded => MapLoader.CurrentMap != null && !MapLoader.IsFallback;
    public static bool IsFallbackMapLoaded => MapLoader.CurrentMap != null && MapLoader.IsFallback;

    public static string MapName => IsFallbackMapLoaded && IsInLobby
        ? LIConstants.MAP_NAME
        : MapLoader.CurrentMap?.name ?? LIConstants.MAP_NAME;
}