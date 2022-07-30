using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Gives credit to map makers
     *      through the Ping Tracker in
     *      the rop right corner.
     */
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            __instance.gameObject.SetActive(true);
            LIMap currentMap = MapLoader.currentMap;
            if (currentMap != null)
            {

                __instance.text.text += "\n" + currentMap.name + " \n";
                if (!string.IsNullOrEmpty(currentMap.authorID))
                    __instance.text.text += "<size=2>by " + currentMap.authorName + "</size>";
                else
                    __instance.text.text += "<size=2><i>(Freeplay Only)</i></size>";
            }
            
        }
    }
}
