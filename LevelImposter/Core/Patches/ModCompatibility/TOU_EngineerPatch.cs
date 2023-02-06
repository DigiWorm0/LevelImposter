using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Fixes the TOU engineer fix
     *      to work in LevelImposter.
     *      
     *      Note: A bug w/ Engineer where the
     *      PlayerId is transmitted instead of the NetId
     *      requires a high Harmony priority to execute
     *      the patch before an exception is thrown
     */
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class TOU_EngineerPatch
    {
        [HarmonyPriority(Priority.High)]
        public static void Postfix([HarmonyArgument(0)] byte callId)
        {
            if (MapLoader.CurrentMap == null)
                return;
            if (!ModCompatibility.IsTOUEnabled)
                return;

            if (callId == (byte)LIRpc.TOU_EngineerFix)
            {
                LILogger.Info("Detected a TOU Engineer Fix");
                LIShipStatus.Instance?.FixAllSabotages();
            }
        }
    }
}