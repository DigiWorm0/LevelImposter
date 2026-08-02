using LevelImposter.AssetLoader;
using LevelImposter.Core.Models;
using LevelImposter.Lobby.Components;
using LevelImposter.Shop.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelImposter.Core;

public static class GameState
{
    // Hardware
    public static bool IsMobile => Application.isMobilePlatform;

    // Map
    public static string MapName => GameConfiguration.CurrentMap?.name ?? LIConstants.MAP_NAME;

    // Scenes
    public static bool IsInFreeplay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInLobby => LobbyBehaviour.Instance != null || LILobbyBehaviour.IsInstance();
    public static bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";
    public static bool IsInShop => ShopManager.Instance != null;
    public static bool IsInMeeting => MeetingHud.Instance != null;
    public static bool IsPlayerLoaded => PlayerControl.LocalPlayer != null;

    // Network
    public static bool IsHost => AmongUsClient.Instance?.AmHost ?? false;

    // Player State
    public static bool IsLocalPlayerImpostor =>
        PlayerControl.LocalPlayer?.Data?.Role.TeamType == RoleTeamTypes.Impostor;

    public static bool IsLocalPlayerDead => PlayerControl.LocalPlayer?.Data?.IsDead ?? true;

    // Loading State
    public static int LoadingAssetsCount => TextureLoader.Instance.QueueSize +
                                            SpriteLoader.Instance.QueueSize +
                                            AudioLoader.Instance.QueueSize;

    public static bool IsLoadingCustomMap => LoadingAssetsCount > 0;
}