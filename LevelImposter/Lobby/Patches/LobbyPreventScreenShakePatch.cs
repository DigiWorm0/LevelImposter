using HarmonyLib;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Shop;
using UnityEngine;

namespace LevelImposter.Lobby;

/*
 *      Prevents the default screen shake behavior when in custom lobbies
 */
[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyPreventScreenShakePatch
{
    private static FollowerCamera? _followerCamera;
    private static float _shakeAmount;
    private static float _shakePeriod;
    
    public static void Prefix()
    {
        _followerCamera = Camera.main?.GetComponent<FollowerCamera>();
        _shakeAmount = _followerCamera?.shakeAmount ?? 0.0f;
        _shakePeriod = _followerCamera?.shakePeriod ?? 0.0f;
    }
    public static void Postfix()
    {
        if (_followerCamera == null || GameConfiguration.CurrentLobbyMap == null)
            return;
        _followerCamera.shakeAmount = _shakeAmount;
        _followerCamera.shakePeriod = _shakePeriod;
    }
}