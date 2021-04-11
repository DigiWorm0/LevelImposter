using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Threading;
using Il2CppSystem.Threading.Tasks;
using InnerNet;
using LevelImposter.DB;
using LevelImposter.Map;
using Reactor;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LevelImposter
{
    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    public static class MapPatch
    {
        public static void Prefix(PolusShipStatus __instance)
        {
            // Load Asset DB
            LILogger.LogInfo("Loading Asset Database...");
            var client = GameObject.Find("NetworkManager").GetComponent<AmongUsClient>();
            foreach (AssetReference assetRef in client.ShipPrefabs)
            {
                if (assetRef.IsDone)
                    AssetDB.ImportMap(assetRef.Asset.Cast<GameObject>());
            }
            LILogger.LogInfo("Asset Database has been Loaded!");

            // Apply Map
            MapApplicator mapApplicator = new MapApplicator();
            mapApplicator.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.Update))]
    public static class SoundPatch
    {
        public static bool Prefix()
        {
            // TODO Add Sounds / Fix Sound Bug
            return false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class DBPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            LILogger.LogInfo("Loading Ship Prefabs...");
            foreach (AssetReference assetRef in __instance.ShipPrefabs)
            {
                assetRef.LoadAsset<GameObject>();
            }
            MapHandler.Load();
        }
    }
    
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
                LILogger.LogWarn("Received map checksum does not match!\n" + checksum + " => " + MapHandler.checksum);

                AmongUsClient client = AmongUsClient.Instance;
                client.LastDisconnectReason = DisconnectReasons.Custom;
                client.LastCustomDisconnect = "Host is using a different map than client";
                client.HandleDisconnect(client.LastDisconnectReason, client.LastCustomDisconnect);
            }
        }
    }

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    public static class VersionPatch
    {
        public static void Postfix(VersionShower __instance)
        {
            __instance.text.Text += "\n\n\n\n\n\n\n[3399FFFF]Level[FF0000FF]Imposter[] v" + MainHarmony.VERSION + " by DigiWorm";
        }
    }
}

    
