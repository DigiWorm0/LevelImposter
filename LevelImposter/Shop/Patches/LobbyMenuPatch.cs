using TMPro;
using HarmonyLib;
using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    /*
     *      Adds downloaded maps to the lobby menu
     */
    [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
    public static class LobbyMenuInitPatch
    {
        public static bool Prefix(KeyValueOption __instance)
        {
            if (__instance.Title == StringNames.GameMapName)
            {
                GameObject.Destroy(__instance);
                __instance.gameObject.AddComponent<LIMapSelector>();
                return false;
            }
            return true;
        }
    }
}
