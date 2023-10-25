using HarmonyLib;
using Hazel;
using LevelImposter.Builders;

namespace LevelImposter.Core
{
    /*
     *      Normally, spore are handled
     *      by FungleShipStatus. This bypasses that
     *      requirement by supplying it's own
     *      spore listings.
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class SporePatchHandle
    {
        public static bool Prefix(
            [HarmonyArgument(0)] byte callId,
            [HarmonyArgument(1)] MessageReader reader,
            PlayerControl __instance)
        {
            if (LIShipStatus.Instance == null)
                return true;
            if (callId != (byte)RpcCalls.TriggerSpores || callId != (byte)RpcCalls.CheckSpore)
                return true;

            // Find spores
            int mushroomId = reader.ReadPackedInt32();
            if (mushroomId < 0 || mushroomId >= SporeBuilder.Mushrooms.Count)
            {
                LILogger.Warn($"[RPC] Could not find a mushroom with ID {mushroomId}");
                return true;
            }

            // Run method
            if (callId == (byte)RpcCalls.TriggerSpores)
                SporeBuilder.Mushrooms[mushroomId].TriggerSpores();
            else
                __instance.CheckSporeTrigger(SporeBuilder.Mushrooms[mushroomId]);

            return false;
        }
    }
}
