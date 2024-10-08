using LevelImposter.Shop;
using UnityEngine.SceneManagement;

namespace LevelImposter.Core;

public static class GameState
{
    // Map State
    public static MapType SelectedMapType => IsInFreeplay
        ? (MapType)AmongUsClient.Instance.TutorialMapId
        : (MapType)GameOptionsManager.Instance.CurrentGameOptions.MapId;

    public static bool IsCustomMapSelected => SelectedMapType == MapType.LevelImposter;
    public static bool IsCustomMapLoaded => MapLoader.CurrentMap != null;
    public static bool IsInCustomMap => LIShipStatus.GetInstanceOrNull() != null;
    public static bool IsFallbackMap => MapLoader.IsFallback;

    public static string MapName => MapLoader.CurrentMap?.name ?? LIConstants.MAP_NAME;

    // Scenes
    public static bool IsInFreeplay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInLobby => LobbyBehaviour.Instance != null;
    public static bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";
    public static bool IsInShop => ShopManager.Instance != null;
    public static bool IsInMeeting => MeetingHud.Instance != null;
    public static bool IsPlayerLoaded => PlayerControl.LocalPlayer != null;

    // Network
    public static bool IsHost => AmongUsClient.Instance?.AmHost ?? false;
}