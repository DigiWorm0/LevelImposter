using System.Collections.Generic;
using System.Linq;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.API;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LevelImposter.Lobby.Sync;

public static class MapRandomizer
{
    private const int FLOATING_POINT_PRECISION = 1000;

    /// <summary>
    ///     Loads a random map from the filesystem and sets it as the current map.
    /// </summary>
    /// <param name="setMapType">If true, sets the current map type to LevelImposter</param>
    public static void RandomizeMap(bool setMapType = true)
    {
        // Check if we're the host
        if (!GameState.IsHost)
            return;

        // Get random map
        var mapData = ChooseRandomMap();
        if (mapData != null)
            GameConfiguration.SetMap(mapData, true);
        else
            GameConfiguration.SetMap(null);

        // Set map type to LevelImposter
        if (setMapType)
            GameConfiguration.SetMapType(MapType.LevelImposter);

        // Sync Map
        GameConfigurationSync.SendGameConfigurationRPC();
    }

    /// <summary>
    ///     Chooses a valid, random map file from the filesystem.
    /// </summary>
    /// <returns>A valid <see cref="MapData" /> object, or null if no valid maps are found.</returns>
    private static MapFile? ChooseRandomMap()
    {
        MapFileAPI.AllMaps.Refresh();
        var remainingMapFiles = MapFileAPI.AllMaps.Get().ToList();
        remainingMapFiles.RemoveAll(map => !IsMapValid(map));

        while (true)
        {
            // Find random map ID
            var mapInfo = ChooseRandomMap(remainingMapFiles);
            if (mapInfo == null)
                return null;

            // Attempt to load map
            var map = MapFileAPI.GetMapData(mapInfo);

            // Validate map file data
            if (map != null)
                return map;

            LILogger.Warn($"Failed to load map [{mapInfo.ID}], skipping.");
            remainingMapFiles.Remove(mapInfo);
        }
    }

    /// <summary>
    ///     Checks if a given map file is valid for use in a multiplayer game.
    /// </summary>
    /// <param name="map">The map file to validate.</param>
    /// <returns>True if the map is valid; otherwise, false.</returns>
    private static bool IsMapValid(MapFileInfo map)
    {
        if (string.IsNullOrEmpty(map.ID))
            return false;

        if (map.MapTarget == MapTarget.Lobby)
            return false;

        if (!map.IsInWorkshop)
            return false;

        return true;
    }

    /// <summary>
    ///     Uses the map weights from the Config API to choose a random map from a list of map files.
    /// </summary>
    /// <param name="allMaps">The list of map files to choose from.</param>
    /// <returns>A random map file, or null if no maps are available.</returns>
    private static MapFileInfo? ChooseRandomMap(List<MapFileInfo> allMaps)
    {
        if (allMaps.Count == 0)
            return null;

        // Get map weights
        var mapWeights = new float[allMaps.Count];
        var sumOfAllMapWeights = 0.0f;
        for (var i = 0; i < allMaps.Count; i++)
        {
            mapWeights[i] = RoundToPrecision(ConfigAPI.GetMapWeight(allMaps[i].ID));
            sumOfAllMapWeights += mapWeights[i];
        }

        // All maps are of zero weight
        if (sumOfAllMapWeights <= 0)
            return null;

        // Choose a random map
        var randomValue = Random.Range(0, sumOfAllMapWeights);
        for (var i = 0; i < allMaps.Count; i++)
        {
            // Check weight
            randomValue -= mapWeights[i];
            if (randomValue >= 0)
                continue;

            return allMaps[i];
        }

        // No map found
        return null;
    }

    /// <summary>
    ///     Rounds a floating point value to the defined precision.
    /// </summary>
    /// <param name="value">Value to round</param>
    /// <returns>Rounded value</returns>
    private static float RoundToPrecision(float value)
    {
        return Mathf.Round(value * FLOATING_POINT_PRECISION) / FLOATING_POINT_PRECISION;
    }
}