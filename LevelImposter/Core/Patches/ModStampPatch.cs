using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Adds the Mod Stamp required by
     *      InnerSloth's modding policy:
     *      https://www.innersloth.com/among-us-mod-policy/
     *      
     *      Also initializes API components
     */
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
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
                apiParent.AddComponent<GitHubAPI>();
                UnityEngine.Object.DontDestroyOnLoad(apiParent);
            }
        }
    }
}
