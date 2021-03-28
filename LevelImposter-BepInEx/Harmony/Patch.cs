using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using LevelImposter.DB;
using LevelImposter.Map;
using Reactor;
using UnityEngine;

namespace LevelImposter
{
    [HarmonyPatch(typeof(PolusShipStatus), nameof(PolusShipStatus.OnEnable))]
    public static class MapPatch
    {
        public static void Postfix(PolusShipStatus __instance)
        {
            MapApplicator mapApplicator = new MapApplicator();
            mapApplicator.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.Update))]
    public static class SoundPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    public static class DBPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            foreach (GameObject obj in __instance.NonAddressableShipPrefabs)
            {
                AssetDB.ImportMap(obj);
            }
            LILogger.LogInfo("Found and stored prefabs");
        }
    }
}
