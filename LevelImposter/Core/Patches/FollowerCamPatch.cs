using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    /*
     *      Disables camera movement while
     *      sprites are still loading in
     */
    [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
    public static class FollowerCamPatch
    {
        public static bool Prefix(FollowerCamera __instance)
        {
            if (SpriteLoader.Instance.RenderCount > 0)
            {
                __instance.centerPosition = __instance.transform.position;
                return false;
            }
            return true;
        }
    }
}