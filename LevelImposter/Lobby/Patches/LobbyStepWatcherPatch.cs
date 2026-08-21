using HarmonyLib;
using LevelImposter.Lobby.Components;

namespace LevelImposter.Lobby.Patches;

/*
 *      By default, step sounds are hard-coded in the lobby to use SkeldShipRoom instead of IStepWatcher.
 *      This patch overrides the default behavior to use the IStepWatcher components instead.
 */
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.PlayStepSound))]
public static class LobbyStepWatcherPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        if (!LILobbyBehaviour.IsInstance())
            return;

        var allStepWatchers = LILobbyBehaviour.GetInstance().AllStepWatchers;
        foreach (var stepWatcher in allStepWatchers)
        {
            var soundGroup = stepWatcher.MakeFootstep(__instance);
            if (!soundGroup)
                continue;

            __instance.FootSteps.clip = soundGroup.Random();
            __instance.FootSteps.Play();
            return;
        }
    }
}