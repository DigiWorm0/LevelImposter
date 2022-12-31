using HarmonyLib;
using UnityEngine;
using LevelImposter.Core;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Gives credit to map makers
     *      through the Ping Tracker in
     *      the top right corner.
     */
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingPatch
    {
        public static void Postfix(PingTracker __instance)
        {
            LIMap currentMap = MapLoader.CurrentMap;
            if (currentMap == null)
                return;
            if (currentMap.properties.showPingIndicator == false)
                return;
            if (!__instance.gameObject.active)
                __instance.gameObject.SetActive(true);

            bool isPublished = !string.IsNullOrEmpty(currentMap.authorID);
            bool isInLobby = LobbyBehaviour.Instance != null;

            __instance.text.text = 
                (isInLobby ? 
                    $"<size=2.5><color=#1a95d8>Level</color><color=#cb2828>Imposter</color> v{LevelImposter.Version}</size>\n" :
                    "") +
                $"<size=2.5>Ping: {AmongUsClient.Instance.Ping}ms</size>\n" +
                $"<color=#1a95d8>{currentMap.name}\n" +
                (isPublished ?
                    $"<size=2.5>by {currentMap.authorName}</size></color>" :
                    $"<size=2.5><i>(Freeplay Only)</i></size></color>");

                /*
                __instance.text.text += $"\n<color=#1a95d8>{currentMap.name} \n";
                if ()
                    __instance.text.text += $"<size=2.5>by {currentMap.authorName}</size> \n";
                else
                    __instance.text.text += "<size=3><i>(Freeplay Only)</i></size></color> \n";
                __instance.text.text += $"<size=1.5>LevelImposter v{LevelImposter.Version}</size>";*/
            
        }
    }
}
