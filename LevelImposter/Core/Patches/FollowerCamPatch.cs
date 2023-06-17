using HarmonyLib;

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