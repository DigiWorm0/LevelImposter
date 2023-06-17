using HarmonyLib;
using UnityEngine;
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
        private const float ANIM_SPEED = 1f;

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
                spriteAnim.Play(clipAnim, ANIM_SPEED);
            }
            else if (gifAnim != null)
            {
                if (on)
                    gifAnim.Play();
                else
                    gifAnim.Stop();
            }
            return false;
        }
    }
}