using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Core;
using PowerTools;
using LevelImposter.Shop;

namespace LevelImposter.Shop
{
    /*
     *      Adds the update button to
     *      the Main Menu screen
     */
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class UpdateButtonPatch
    {
        public static void Postfix()
        {
            GitHubAPI.Instance.GetLatestRelease((release) => {
                bool isCurrent = GitHubAPI.Instance.IsCurrent(release);
                if (!isCurrent)
                    UpdateButtonBuilder.Build();
            }, (error) => {});
        }
    }
}