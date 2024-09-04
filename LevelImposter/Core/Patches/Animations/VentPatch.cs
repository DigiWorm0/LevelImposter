using HarmonyLib;
using PowerTools;

namespace LevelImposter.Core;

//// <summary>
/// Toggles a GIF animation alongside the
/// regular animation components on vents.
/// </summary>
[HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
public static class EnterVentPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl pc, Vent __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        // Player
        if (pc.AmOwner)
        {
            Vent.currentVent = __instance;
            ConsoleJoystick.SetMode_Vent();
        }

        // Animation
        var spriteAnim = __instance.GetComponent<SpriteAnim>();
        var gifAnim = __instance.GetComponent<GIFAnimator>();
        if (spriteAnim != null && __instance.EnterVentAnim != null)
            spriteAnim.Play(__instance.EnterVentAnim);
        else if (gifAnim != null)
            gifAnim.Play(false, false);
        else
            return false;

        // Sound
        if (pc.AmOwner && Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
            SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false).pitch =
                FloatRange.Next(0.8f, 1.2f);
        }

        return false;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))]
public static class ExitVentPatch
{
    public static bool Prefix([HarmonyArgument(0)] PlayerControl pc, Vent __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        // Player
        if (pc.AmOwner) Vent.currentVent = null;

        // Animation
        var spriteAnim = __instance.GetComponent<SpriteAnim>();
        var gifAnim = __instance.GetComponent<GIFAnimator>();
        if (spriteAnim != null && __instance.ExitVentAnim != null)
            spriteAnim.Play(__instance.ExitVentAnim);
        else if (gifAnim != null)
            gifAnim.Play(false, false);
        else
            return false;

        // Sound
        if (pc.AmOwner && Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
            SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false).pitch =
                FloatRange.Next(0.8f, 1.2f);
        }

        return false;
    }
}