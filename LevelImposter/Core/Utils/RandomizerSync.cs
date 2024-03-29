﻿using System;
using Reactor.Networking.Attributes;

namespace LevelImposter.Core
{
    /// <summary>
    /// A randomizer class that is synced across all clients
    /// </summary>
    public static class RandomizerSync
    {
        private static int _randomSeed = 0;

        /// <summary>
        /// Gets a random value based on an element GUID.
        /// Given the same GUID, weight, and seed will always return with the same value.
        /// Random seeds are synchronized it across all clients and
        /// regenerated with <c>MapUtils.SyncRandomSeed()</c>
        /// </summary>
        /// <param name="id">GUID identifier</param>
        /// <param name="weight">A weight value to generate new numbers with the same GUID</param>
        /// <returns>A random float between 0.0 and 1.0 (inclusive)</returns>
        public static float GetRandom(Guid id, int weight = 0)
        {
            int trueSeed = id.GetHashCode() + _randomSeed + weight;
            UnityEngine.Random.InitState(trueSeed);
            return UnityEngine.Random.value;
        }

        /// <summary>
        /// Generates a new random seed and
        /// synchronizes it across all clients.
        /// </summary>
        public static void SyncRandomSeed()
        {
            bool isConnected = AmongUsClient.Instance.AmConnected;
            bool isHost = AmongUsClient.Instance.AmHost;
            if (isConnected && (!isHost || PlayerControl.LocalPlayer == null))
                return;
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            int newSeed = UnityEngine.Random.RandomRange(int.MinValue, int.MaxValue);
            if (isConnected)
                RPCSyncRandomSeed(PlayerControl.LocalPlayer, newSeed);
            else
                _randomSeed = newSeed;
        }
        [MethodRpc((uint)LIRpc.SyncRandomSeed)]
        private static void RPCSyncRandomSeed(PlayerControl _, int randomSeed)
        {
            LILogger.Info($"[RPC] New random seed set: {randomSeed}");
            _randomSeed = randomSeed;
        }
    }
}
