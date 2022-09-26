using HarmonyLib;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib.Attributes;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Adds the Mod Stamp required by
     *      InnerSloth's modding policy:
     *      https://www.innersloth.com/among-us-mod-policy/
     */
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class ModStampPatch
    {
        public static void Postfix()
        {
            ModManager.Instance.ShowModStamp();

            if (LevelImposterAPI.Instance == null)
            {
                GameObject apiParent = new GameObject("LevelImposter API");
                apiParent.AddComponent<LevelImposterAPI>();
                apiParent.AddComponent<MapFileAPI>();
                apiParent.AddComponent<ThumbnailFileAPI>();
                UnityEngine.Object.DontDestroyOnLoad(apiParent);
            }
        }
    }
}
