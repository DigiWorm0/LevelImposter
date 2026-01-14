using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using LevelImposter.Core;

namespace LevelImposter.FileIO;

/// <summary>
///     API to read and write from local config file
/// </summary>
public static class ConfigAPI
{
    private static ConfigData _configData = new();

    /// <summary>
    ///     Gets the current directory where config file is stored.
    ///     Usually in a LevelImposter folder beside the LevelImposter.dll.
    /// </summary>
    /// <returns>String path where config file is stored.</returns>
    public static string GetDirectory()
    {
        var gameDir = Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
        return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/config.json");
    }

    /// <summary>
    ///     Reads all data from the configuration file
    /// </summary>
    public static void ReadAll()
    {
        var directory = GetDirectory();
        if (!File.Exists(directory))
            return;
        var configJSON = File.ReadAllText(directory);
        _configData = JsonSerializer.Deserialize<ConfigData>(configJSON) ?? new ConfigData();
    }

    /// <summary>
    ///     Saves all data to the configuration file.
    ///     This is not done automatically!
    /// </summary>
    public static void Save()
    {
        try
        {
            LILogger.Info("Saving local config file");
            var path = GetDirectory();
            var directory = Path.GetDirectoryName(path) ?? path;

            // Create Directory
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Write File
            var configJSON = JsonSerializer.Serialize(_configData);
            File.WriteAllText(path, configJSON);
        }
        catch (Exception e)
        {
            LILogger.Error("Failed to save local config file");
            LILogger.Info(e);
        }
    }

    /// <summary>
    ///     Gets the random weight of a map
    /// </summary>
    /// <param name="mapID">Map ID to get weight of</param>
    /// <returns>Value from 0 to 1</returns>
    public static float GetMapWeight(string mapID)
    {
        if (_configData.RandomWeights?.ContainsKey(mapID) == true)
            return _configData.RandomWeights[mapID];
        return 0.5f;
    }

    /// <summary>
    ///     Sets the random weight of a map
    /// </summary>
    /// <param name="mapID">Map ID to set the weight of</param>
    /// <param name="weight">Value from 0 to 1</param>
    public static void SetMapWeight(string mapID, float weight)
    {
        if (_configData.RandomWeights == null)
            _configData.RandomWeights = new Dictionary<string, float>();
        if (_configData.RandomWeights.ContainsKey(mapID))
            _configData.RandomWeights[mapID] = weight;
        else
            _configData.RandomWeights.Add(mapID, weight);
    }

    /// <summary>
    ///     Gets the Map ID of the last opened map
    /// </summary>
    /// <returns>Map ID or null if none used</returns>
    public static string? GetLastMapID()
    {
        return _configData.LastMapJoined;
    }

    /// <summary>
    ///     Sets the Map ID of the last opened map
    /// </summary>
    /// <param name="mapID">Map ID or null if none used</param>
    public static void SetLastMapID(string? mapID)
    {
        if (_configData.LastMapJoined == mapID)
            return;
        _configData.LastMapJoined = mapID;
        Save();
    }

    [Serializable]
    public class ConfigData
    {
        public string? LastMapJoined { get; set; }
        public Dictionary<string, float>? RandomWeights { get; set; }
    }
}