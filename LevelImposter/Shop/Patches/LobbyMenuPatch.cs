using AmongUs.GameOptions;
using HarmonyLib;
using LevelImposter.Core;

namespace LevelImposter.Shop;

/*
 *      Resets the map to Skeld when creating a new lobby
 */
[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
public static class LastMapValuePatch
{
    public static void Postfix()
    {
        var options = GameOptionsManager.Instance.CurrentGameOptions;
        if (options.MapId != (byte)MapType.LevelImposter)
            return;

        GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, (byte)MapType.Skeld);
        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
    }
}

/*
 *      Initializes a new Map Console in the Lobby
 */
[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyMenuInitPatch
{
    public static void Postfix()
    {
        LobbyConsoleBuilder.Build();
    }
}