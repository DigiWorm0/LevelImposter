using HarmonyLib;

namespace LevelImposter.Core
{
    /// <summary>
    /// Disables camera movement while still in loading screen.
    /// </summary>
    [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
    public static class LoadingCameraPatch
    {
        public static bool Prefix(FollowerCamera __instance)
        {
            if (SpriteLoader.Instance == null || LIShipStatus.Instance == null)
                return true;

            if (SpriteLoader.Instance.RenderCount > 0)
            {
                __instance.centerPosition = __instance.transform.position;
                return false;
            }
            return true;
        }
    }
}