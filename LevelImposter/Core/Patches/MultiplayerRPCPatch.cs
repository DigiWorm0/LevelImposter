using System;
using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;
using Hazel;

namespace LevelImposter.Core.Patches
{
    /*
     *      Transmits and receives LevelImposter
     *      map information over RPC
     */
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static class SendRpcPatch
    {
        public const int RPC_ID = 149; // ASCII code of the letter L because why not

        public static void Postfix(AmongUsClient __instance)
        {
            if (__instance.AmHost)
            {
                LILogger.Info("[RPC] Player Joined: Sending map ID...");
                var writer = __instance.StartRpcImmediately(
                    PlayerControl.LocalPlayer.NetId,
                    RPC_ID,
                    SendOption.Reliable,
                    -1
                );
                Guid mapID = Guid.Empty;
                if (MapLoader.currentMap != null)
                    mapID = MapLoader.currentMap.id;
                writer.Write(mapID.ToByteArray());
                __instance.FinishRpcImmediately(writer);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class ReceiveRpcPatch
    {
        public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (callId != SendRpcPatch.RPC_ID)
                return;

            byte[] mapBytes = reader.ReadBytes(16);
            Guid mapID = new Guid(mapBytes);
            if (mapID.Equals(Guid.Empty))
            {
                LILogger.Info("[RPC] Received blank map ID. Unloading map...");
                MapLoader.UnloadMap();
            }
            else
            {
                
                LILogger.Info("[RPC] Recieved map ID: " + mapID);
                if (!MapLoader.Exists(mapID.ToString()))
                {
                    LILogger.Info("[RPC] Downloading map...");
                    MapAPI.DownloadMap(mapID, (System.Action<string>)((string mapJson) =>
                    {
                        LILogger.Info("[RPC] Loading map...");
                        MapLoader.WriteMap(mapID.ToString(), mapJson);
                        MapLoader.LoadMap(mapID.ToString());
                    }));
                }
                else
                {
                    LILogger.Info("[RPC] Loading map...");
                    MapLoader.LoadMap(mapID.ToString());
                }
            }
        }
    }

}
