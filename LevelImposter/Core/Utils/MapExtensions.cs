using System.Linq;
using UnityEngine;

namespace LevelImposter.Core;

public static class MapExtensions
{
    /// <summary>
    ///     Finds a sound of the cooresponding type from a list of LISounds
    /// </summary>
    /// <param name="sounds">List of LI Sounds from serialized map data</param>
    /// <param name="type">The type of sound to search for</param>
    /// <returns>The first sound of the cooresponding type or null if none found</returns>
    public static LISound? FindSound(this LISound[]? sounds, string type)
    {
        return sounds?.FirstOrDefault(sound => sound.type == type);
    }

    /// <summary>
    ///     Adjusts a Vector3 position's Z value by its Y value
    ///     such that the player is always on Z=-5.
    ///     Converts LI coordinates to Unity coordinates.
    /// </summary>
    /// <param name="vector">Vector to scale</param>
    /// <returns>Vector with adjusted Z</returns>
    public static Vector3 ScaleZPositionByY(this Vector3 vector)
    {
        return vector - new Vector3(0, 0, -(vector.y / 1000.0f) + LIConstants.PLAYER_POS);
    }
}