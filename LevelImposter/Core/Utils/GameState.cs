using LevelImposter.AssetLoader;
using LevelImposter.Core.Translations;
using LevelImposter.Lobby.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelImposter.Core.Utils;

public static class GameState
{
    // Hardware
    public static bool IsMobile => Application.isMobilePlatform;

    // Map
    public static string MapName => GameConfiguration.CurrentMap?.name ?? Translation.Get("lobby.random_custom_map");

    // Scenes
    public static bool IsInFreeplay => AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay;
    public static bool IsInLobby => LobbyBehaviour.Instance != null || LILobbyBehaviour.IsInstance();
    public static bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";
    public static bool IsInMeeting => MeetingHud.Instance != null;

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