using HarmonyLib;
using LevelImposter.FileIO;
using LevelImposter.Shop;

namespace LevelImposter.Lobby;

/*
 *      Initializes a new Map Console in the Lobby
 */

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class AddLILobbyBehaviourPatch
{
    public static void Postfix(LobbyBehaviour __instance)
    {
        // DEBUG TEST REMOVE ME
        GameConfiguration.SetLobbyMap(MapFileAPI.Get("Lobby Dropship Map"));
        
        __instance.gameObject.AddComponent<LILobbyBehaviour>();
    }
}