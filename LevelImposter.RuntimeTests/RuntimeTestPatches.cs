using HarmonyLib;

namespace LevelImposter.RuntimeTests;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
internal static class MainMenuManagerStartPatch
{
    private static void Postfix()
    {
        RuntimeTestRunner.StartAll();
    }
}

[HarmonyPatch(typeof(AmongUsClient), "Update")]
internal static class AmongUsClientUpdatePatch
{
    private static void Postfix()
    {
        RuntimeTestRunner.Tick();
    }
}
