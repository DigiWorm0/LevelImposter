using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using LevelImposter.Builders.Lobby;
using LevelImposter.Core.Utils;
using UnityEngine;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;

/*
 *      A lot of the behavior of PlayerPhysics.CoSpawnPlayer
 *      is hard-coded for Among Us's dropship lobby.
 *      This patch prevents the default behavior from running when in a custom lobby.
 */
namespace LevelImposter.Lobby.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
public static class LobbySpawnPatch
{
    public static readonly Dictionary<uint, Il2CppIEnumerator> SpawnCoroutines = new();

    public static bool Prefix(
        ref Il2CppIEnumerator __result,
        PlayerPhysics __instance
    )
    {
        if (GameConfiguration.CurrentLobbyMap == null)
            return true;

        __result = CoSpawnPlayer(__instance).WrapToIl2Cpp();
        return false;
    }

    public static void Postfix(
        ref Il2CppIEnumerator __result,
        PlayerPhysics __instance
    )
    {
        SpawnCoroutines[__instance.myPlayer.PlayerId] = __result;
    }

    private static IEnumerator CoSpawnPlayer(PlayerPhysics player)
    {
        // Get Lobby Behavior
        var lobby = LobbyBehaviour.Instance;
        if (!lobby)
            yield break;

        // Enable InputHandler
        var myPlayer = player.myPlayer;
        if (myPlayer.AmOwner)
            player.inputHandler.enabled = true;

        // Get Spawn Position
        var spawnPoint = LobbySpawnBuilder.GetSpawnPoint(myPlayer);
        var spawnPosition = player.Vec2ToPosition(spawnPoint.Position);

        // Temporarily disable components
        myPlayer.cosmetics.ToggleName(false);
        myPlayer.Collider.enabled = false;
        myPlayer.NetTransform.enabled = false;
        KillAnimation.SetMovement(myPlayer, false);

        // Apply visual cosmetics
        yield return new WaitForFixedUpdate();
        myPlayer.cosmetics.SetForcedVisible(true);
        player.FlipX = spawnPoint.IsFlipped;

        // Play spawn sound
        SoundManager.Instance.PlaySound(
            lobby.SpawnSound,
            false,
            0.75f);

        // Play Skin Animation
        if (spawnPoint.PlaySkinAnimation)
            myPlayer.cosmetics.AnimateSkinSpawn();

        // Play spawn animation
        myPlayer.transform.position = spawnPosition;
        if (spawnPoint.PlaySpawnAnimation)
        {
            yield return player.Animations.CoPlaySpawnAnimation(player.FlipX);
            player.transform.position = spawnPosition + new Vector3(spawnPoint.IsFlipped ? -0.3f : 0.3f, -0.24f);
            player.enabled = true;
            player.ResetAnimState();
            yield return player.WalkPlayerTo(spawnPosition);
        }

        // Re-enable components
        player.enabled = true;
        player.ResetMoveState(false);
        myPlayer.Collider.enabled = true;
        myPlayer.moveable = true;
        myPlayer.NetTransform.ClearPositionQueues();
        myPlayer.cosmetics.ToggleName(true);

        // Disable InputHandler
        if (myPlayer.AmOwner)
            player.inputHandler.enabled = false;
    }
}

/**
 * By default, CoSpawnPlayer is inlined so the patch never executes.
 * This is a hack to ensure the patch always executes.
 */
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
public static class PlayerControlStartPatch
{
    private static bool _isNew;

    public static void Prefix(PlayerControl __instance)
    {
        _isNew = __instance.isNew;
        __instance.isNew = false;
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (_isNew)
            __instance.StartCoroutine(__instance.MyPhysics.CoSpawnPlayer(LobbyBehaviour.Instance));
    }
}