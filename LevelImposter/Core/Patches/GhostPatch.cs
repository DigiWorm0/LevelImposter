using HarmonyLib;
using UnityEngine;
using LevelImposter.Shop;
using Hazel;

namespace LevelImposter.Core
{
    /*
     *      Raises Ghost Z height
     *      above map elements
     */
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
    public static class GhostPatch
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (MapLoader.CurrentMap == null || !__instance.myPlayer.Data.IsDead)
                return;

            __instance.transform.position += new Vector3(
                0,
                0,
                -2.0f
            );
        }
    }
}
