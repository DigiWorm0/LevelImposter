using System;
using System.Collections.Generic;
using LevelImposter.Core;
using LevelImposter.FileIO;
using Random = UnityEngine.Random;

namespace LevelImposter.Shop;

public static class MapRandomizer
{
    /// <summary>
    /// Loads a random map from the filesystem and sets it as the current map.
    /// </summary>
    /// <param name="setMapType">If true, sets the current map type to LevelImposter</param>
    public static void RandomizeMap(bool setMapType = true)
    {
        // Check if we're the host
        if (!GameState.IsHost)
            return;
        
        // Get random map
        var map = GetRandomMap();
        if (map != null)
            GameConfiguration.SetMap(map, true);
        else
            GameConfiguration.SetMap(null);
        
        // Set map type to LevelImposter
        if (setMapType)
            GameConfiguration.SetMapType(MapType.LevelImposter);
    }
    
    /// <summary>
    /// Attempts to find and load a random map from the filesystem, excluding blacklisted maps.
    /// </summary>
    /// <param name="blacklistMaps">List of map IDs to exclude from selection</param>
    /// <returns>Loaded LIMap or null if none could be found</returns>
    private static LIMap? GetRandomMap(List<string>? blacklistMaps = null)
    {
        while (true)
        {
            // Initialize blacklist
            blacklistMaps ??= [];

            // Find random map ID
            var mapID = RecursivelyFindRandomMapID(blacklistMaps);
            if (mapID == null)
                return null;

            // Attempt to load map
            var map = MapFileAPI.Get(mapID);
            if (map != null)
                return map;

            // Failed to load map, blacklist and try again
            LILogger.Warn($"Map randomizer could not load map [{mapID}] from filesystem.");
            blacklistMaps.Add(mapID);
        }
    }

    /// <summary>
    /// Recursively finds a random map ID among the currently installed custom maps (excluding blacklisted and local-only maps).
    /// Utilizes map weights from the config.
    /// </summary>
    /// <param name="blacklistMaps">List of map IDs to exclude from selection</param>
    /// <returns></returns>
    /// <exception cref="Exception">If no valid map could be found</exception>
    private static string? RecursivelyFindRandomMapID(List<string>? blacklistMaps)
    {
        // Initialize blacklist
        blacklistMaps ??= [];
        
        // Get all custom maps
        var fileIDs = new List<string>(MapFileAPI.ListIDs());
        var mapIDs = fileIDs.FindAll(id => !blacklistMaps.Contains(id));
        if (mapIDs.Count <= 0)
            return null;    // <-- No valid maps left

        // Get map weights
        var mapWeights = new float[mapIDs.Count];
        var sumOfAllMapWeights = 0.0f;
        for (var i = 0; i < mapIDs.Count; i++)
        {
            var mapWeight = ConfigAPI.GetMapWeight(mapIDs[i]);
            mapWeights[i] = sumOfAllMapWeights + mapWeight;
            sumOfAllMapWeights += mapWeight;
        }

        // All maps are of zero weight
        if (sumOfAllMapWeights <= 0)
            return null;

        // Choose a random map
        var randomValue = Random.Range(0, sumOfAllMapWeights);
        var remainingWeight = sumOfAllMapWeights;
        for (var i = 0; i < mapIDs.Count; i++)
        {
            // Check weight
            remainingWeight -= mapWeights[i];
            if (remainingWeight > 0)
                continue;
            
            // Check if map is in workshop
            var mapID = mapIDs[i];
            var isInWorkshop = Guid.TryParse(mapID, out _);
            if (isInWorkshop)
                return mapID;

            // Blacklist local-only maps and try again
            blacklistMaps.Add(mapID);
            return RecursivelyFindRandomMapID(blacklistMaps);
        }

        // (Code should never reach here)
        return null;
    }
}