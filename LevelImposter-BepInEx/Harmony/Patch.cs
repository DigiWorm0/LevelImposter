using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Threading;
using Il2CppSystem.Threading.Tasks;
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
        }
    }
}
