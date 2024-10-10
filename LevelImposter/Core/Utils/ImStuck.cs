using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

public class ImStuck
{
    public static readonly KeyCode[] RESPAWN_SEQ =
    {
        KeyCode.I,
        KeyCode.M,
        KeyCode.S,
        KeyCode.T,
        KeyCode.U,
        KeyCode.C,
        KeyCode.K
    };

    public void Init()
    {
        // Get the ShipStatus instance
        var shipStatus = LIShipStatus.GetInstance();

        // Respawn the player on key combo
        shipStatus.StartCoroutine(
            CoHandleKeyCombo(
                RESPAWN_SEQ,
                () => { RespawnPlayer(PlayerControl.LocalPlayer); }
            ).WrapToIl2Cpp()
        );
    }

    /// <summary>
    ///     Coroutine to respawn the player
    ///     with a specific key combo. (Shift + "RES" or Shift + "CPU")
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoHandleKeyCombo(KeyCode[] sequence, Action onSequence)
    {
        var state = 0;

        while (true)
        {
            var shift = Input.GetKey(KeyCode.LeftShift)
                        || Input.GetKey(KeyCode.RightShift);
            var seqKey = Input.GetKeyDown(sequence[state]);
            var backKey = Input.GetKeyDown(KeyCode.Backspace);

            if (shift && seqKey)
                state++;
            else if (!shift || backKey) state = 0;

            if (state >= sequence.Length)
            {
                state = 0;
                onSequence.Invoke();
            }

            yield return null;
        }
    }

    /// <summary>
    ///     Resets the Local Player to the
    ///     ShipStatus's spawn location in the
    ///     event they are stuck.
    /// </summary>
    /// <param name="playerControl">Player to respawn</param>
    [MethodRpc((uint)LIRpc.ResetPlayer)]
    private static void RespawnPlayer(PlayerControl playerControl)
    {
        var shipStatus = LIShipStatus.GetShip();
        if (playerControl == null || shipStatus == null)
            return;
        LILogger.Info($"Resetting {playerControl.name} to spawn");
        var playerPhysics = playerControl.GetComponent<PlayerPhysics>();
        playerPhysics.transform.position = shipStatus.InitialSpawnCenter;
        if (playerPhysics.AmOwner)
        {
            playerPhysics.ExitAllVents();
            LILogger.Notify("You've been reset to spawn", false);
        }
    }
}