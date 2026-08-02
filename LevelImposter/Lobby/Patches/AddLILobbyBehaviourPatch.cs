using HarmonyLib;
using LevelImposter.Lobby.Components;

namespace LevelImposter.Lobby.Patches;

/*
 *      Appends the custom LevelImposter lobby logic to the current LobbyBehaviour
 */
[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class AddLILobbyBehaviourPatch
{
    public static void Postfix(LobbyBehaviour __instance)
    {
        __instance.gameObject.AddComponent<LILobbyBehaviour>();
    }
}