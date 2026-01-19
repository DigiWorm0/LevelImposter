using HarmonyLib;

namespace LevelImposter.Lobby;

/*
 *      Initializes a new Map Console in the Lobby
 */

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class AddLILobbyBehaviourPatch
{
    public static void Postfix(LobbyBehaviour __instance)
    {
        __instance.gameObject.AddComponent<LILobbyBehaviour>();
    }
}