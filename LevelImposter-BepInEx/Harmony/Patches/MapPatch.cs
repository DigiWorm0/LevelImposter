using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LevelImposter.Harmony.Patches
{
    
    [HarmonyPatch(typeof(AspectSize), nameof(AspectSize.OnEnable))]
    public static class AspectPatch
    {
        public static bool Prefix()
        {
            return ShipStatus.Instance.name != "PolusShip(Clone)";
        }
    }
    
    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    public static class MapPatch
    {
        public static void Postfix(PolusShipStatus __instance)
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
}
