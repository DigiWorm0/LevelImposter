using HarmonyLib;
using Hazel;
using InnerNet;
using LevelImposter.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Harmony
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static class SendRpc
    {
        public static void Postfix(AmongUsClient __instance)
        {
            if (__instance.AmHost)
            {
                LILogger.LogInfo("Player Joined: Sending map checksum...");
                var writer = __instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 42, SendOption.Reliable, -1);
                writer.Write(MapHandler.checksum);
                __instance.FinishRpcImmediately(writer);
            }

        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class ReceiveRpc
    {
        public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId != 42)
                return;

            string checksum = reader.ReadString();
            if (checksum != MapHandler.checksum)
            {
                LILogger.LogWarn("Received map checksum does not match! (" + checksum + " => " + MapHandler.checksum + ")");

                AmongUsClient client = AmongUsClient.Instance;
                client.LastDisconnectReason = DisconnectReasons.Custom;
                client.LastCustomDisconnect = "Host is using a different map than client";
                client.HandleDisconnect(client.LastDisconnectReason, client.LastCustomDisconnect);
            }
        }
    }
}
