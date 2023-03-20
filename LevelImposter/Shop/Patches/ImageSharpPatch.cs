using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Core;
using PowerTools;
using LevelImposter.Shop;
using Twitch;
using TMPro;

namespace LevelImposter.Shop
{
    /*
     *      Prompts if ImageSharp is missing
     *      on the Main Menu screen
     */
    [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.CloseStartupWaitScreen))]
    public static class ImageSharpPatch
    {
        public static void Postfix()
        {
            if (ImageSharpWrapper.IsInstalled)
                return;

            LILogger.Error($"{ImageSharpWrapper.DLL_NAME} is missing from the plugins directory");

            GameObject? popupPrefab = TwitchManager.Instance?.TwitchPopup?.gameObject;
            if (popupPrefab == null)
                return;
            GameObject popupObject = UnityEngine.Object.Instantiate(popupPrefab);
            GenericPopup popupComponent = popupObject.GetComponent<GenericPopup>();
            TextMeshPro popupText = popupComponent.TextAreaTMP;
            popupText.enableAutoSizing = false;
            popupComponent.Show($"<color=yellow>Warning!</color>\n<size=1.5>Missing {ImageSharpWrapper.DLL_NAME}\nMaps may not load properly</size>");
        }
    }
}