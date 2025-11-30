using HarmonyLib;
using PowerTools;

namespace LevelImposter.Core;

/// <summary>
///     Toggles a GIF animation alongside the
///     regular animation components on surveillance cameras.
/// </summary>
[HarmonyPatch(typeof(SurvCamera), nameof(SurvCamera.SetAnimation))]
public static class CamAnimationPatch
{
    private const float ANIM_SPEED = 1f;

    public static bool Prefix([HarmonyArgument(0)] bool on, SurvCamera __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        // Animation
        var spriteAnim = __instance.GetComponent<SpriteAnim>();
        var animator = __instance.GetComponent<LIAnimatorBase>();
        var clipAnim = on ? __instance.OnAnim : __instance.OffAnim;
        if (spriteAnim != null && clipAnim != null)
        {
            spriteAnim.Play(clipAnim);
        }
        else if (animator != null)
        {
            if (on)
                animator.Play();
            else
                animator.Stop();
        }

        return false;
    }
}