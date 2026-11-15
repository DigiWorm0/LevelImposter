using System.Linq;
using LevelImposter.MapFormat.Models;
using UnityEngine;

namespace LevelImposter.Core.Utils;

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

    /// <summary>
    ///     Converts an LI map point to a Unity vector.
    /// </summary>
    /// <param name="point">The LI map point.</param>
    /// <returns>The point as a <see cref="Vector2" />.</returns>
    public static Vector2 ToUnity(this Point point)
    {
        return new Vector2(point.x, point.y);
    }

    /// <summary>
    ///     Converts an LI map color to a Unity color.
    /// </summary>
    /// <param name="color">The LI map color.</param>
    /// <returns>The color as a <see cref="Color" />.</returns>
    public static Color ToUnity(this LIColor color)
    {
        return new Color(
            color.r / 255.0f,
            color.g / 255.0f,
            color.b / 255.0f,
            color.a
        );
    }
}