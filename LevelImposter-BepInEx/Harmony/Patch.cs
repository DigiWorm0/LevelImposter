using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using LevelImposter.Map;
using LevelImposter.Models;
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
}
