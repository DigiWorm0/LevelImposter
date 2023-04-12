using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;

namespace LevelImposter.Core
{
    /*
     *      Synchronizes a random seed
     *      value to all connected clients
     */
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static class RandomStartPatch
    {
        public static void Postfix()
        {
            MapUtils.SyncRandomSeed();
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    public static class RandomJoinPatch
    {
        public static void Postfix()
        {
            RandomStartPatch.Postfix();
        }
    }
}