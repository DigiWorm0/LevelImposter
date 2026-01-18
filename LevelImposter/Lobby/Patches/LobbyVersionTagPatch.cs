using HarmonyLib;
using UnityEngine;

namespace LevelImposter.Lobby;

/*
 *      Gives credit to map makers
 *      through on-screen text in
 *      the bottom left corner of the screen
 */
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
public static class LobbyVersionTagPatch
{
    public static void Postfix(HudManager __instance)
    {
        if (LobbyVersionTag.IsInitialized)
            return;

        // Create Lobby Version Tag
        var lobbyVersionTag = new GameObject("LobbyVersionTag");
        lobbyVersionTag.AddComponent<LobbyVersionTag>();
        lobbyVersionTag.transform.SetParent(__instance.transform);
    }
}