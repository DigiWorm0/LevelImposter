using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;
using PowerTools;

namespace LevelImposter.Core
{
    /*
     *      Disables camera movement while
     *      sprites are still loading in
     */
    [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
    public static class FollowerCamPatch
    {
        public static bool Prefix()
        {
            return LISpriteLoader.RenderCount <= 0;
        }
    }
}