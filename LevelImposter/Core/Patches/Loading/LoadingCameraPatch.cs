using HarmonyLib;
using LevelImposter.AssetLoader;

namespace LevelImposter.Core;

/// <summary>
///     Disables camera movement while still in loading screen.
/// </summary>
[HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
public static class LoadingCameraPatch
{
    public static bool Prefix(FollowerCamera __instance)
    {
        if (!LIShipStatus.IsInstance())
            return true;

        if (SpriteLoader.Instance.QueueSize <= 0)
            return true;

        __instance.centerPosition = __instance.transform.position;
        return false;
    }
}