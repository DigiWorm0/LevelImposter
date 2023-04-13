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
     *      This enables the use of
     *      LevelImposter.Core.GIFAnimator
     *      instead of PowerTools.SpriteAnim
     */
    [HarmonyPatch(typeof(SurvCamera), nameof(SurvCamera.SetAnimation))]
    public static class CamAnimationPatch
    {
        public static bool Prefix([HarmonyArgument(0)] bool on, SurvCamera __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;

            // Animation
            SpriteAnim spriteAnim = __instance.GetComponent<SpriteAnim>();
            GIFAnimator gifAnim = __instance.GetComponent<GIFAnimator>();
            AnimationClip clipAnim = on ? __instance.OnAnim : __instance.OffAnim;
            if (spriteAnim != null && clipAnim != null)
            {
                spriteAnim.Play(clipAnim, 1f);
            }
            else if (gifAnim != null)
            {
                if (on)
                    gifAnim.Play(true);
                else
                    gifAnim.Stop();
            }
            return false;
        }
    }
}