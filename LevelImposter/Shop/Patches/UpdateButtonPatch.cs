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
        private static bool _isCurrent = true;
        public static void Postfix()
        {
            if (_isCurrent)
            {
                GitHubAPI.GetLatestRelease((release) => {
                    _isCurrent = GitHubAPI.IsCurrent(release);
                    if (!_isCurrent)
                        UpdateButtonBuilder.Build();
                }, (error) => {
                    _isCurrent = true;
                });
            }
            else
            {
                UpdateButtonBuilder.Build();
            }
        }
    }
}