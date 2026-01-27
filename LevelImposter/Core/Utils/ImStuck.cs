using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Networking;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
/// Handles the "I'm Stuck" respawn functionality
/// </summary>
public static class ImStuck
{
    private static readonly KeyCode[] RespawnSeq = [
        KeyCode.I,
        KeyCode.M,
        KeyCode.S,
        KeyCode.T,
        KeyCode.U,
        KeyCode.C,
        KeyCode.K
    ];

    public static void Init()
    {
        // Respawn the player on key combo
        Coroutines.Start(CoHandleKeyCombo(RespawnSeq, SendResetPlayerRPC));
    }

    /// <summary>
    /// Sends an RPC to reset the player
    /// </summary>
    private static void SendResetPlayerRPC()
    {
        Rpc<ResetPlayerRPC>.Instance.Send(true, true);
    }

    /// <summary>
    ///     Coroutine to respawn the player
    ///     with a specific key combo. (Shift + "RES" or Shift + "CPU")
    /// </summary>
    [HideFromIl2Cpp]
    private static IEnumerator CoHandleKeyCombo(KeyCode[] sequence, Action onSequence)
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
}