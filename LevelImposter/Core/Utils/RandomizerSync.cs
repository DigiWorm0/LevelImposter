using System;
using Reactor.Networking.Attributes;
using Random = UnityEngine.Random;

namespace LevelImposter.Core;

/// <summary>
///     A randomizer class that is synced across all clients
/// </summary>
public static class RandomizerSync
{
    private static int _randomSeed;

    /// <summary>
    ///     Gets a random value based on an element GUID.
    ///     Given the same GUID, weight, and seed will always return with the same value.
    ///     Random seeds are synchronized it across all clients and
    ///     regenerated with <c>MapUtils.SyncRandomSeed()</c>
    /// </summary>
    /// <param name="id">GUID identifier</param>
    /// <param name="weight">A weight value to generate new numbers with the same GUID</param>
    /// <returns>A random float between 0.0 and 1.0 (inclusive)</returns>
    public static float GetRandom(Guid id, int weight = 0)
    {
        // Generate a new seed based on the GUID and weight
        var trueSeed = id.GetHashCode() + _randomSeed + weight;
        Random.InitState(trueSeed);

        // Generate a random value
        var randomValue = Random.value;

        // Reset the seed to a pseudo-random value to avoid predictability
        Random.InitState((int)DateTime.Now.Ticks);

        return randomValue;
    }

    /// <summary>
    /// Generates a new random seed based on the current time.
    /// </summary>
    /// <returns>A new random seed integer.</returns>
    public static int GenerateRandomSeed()
    {
        Random.InitState((int)DateTime.Now.Ticks);
        _randomSeed = Random.RandomRange(int.MinValue, int.MaxValue);
        return _randomSeed;
    }

    /// <summary>
    /// Sets the random seed to a specific value.
    /// </summary>
    /// <param name="seed">The seed value to set.</param>
    public static void SetRandomSeed(int seed)
    {
        _randomSeed = seed;
    }
}