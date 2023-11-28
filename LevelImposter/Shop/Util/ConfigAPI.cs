using LevelImposter.Core;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to read and write from local config file
    /// </summary>
    public static class ConfigAPI
    {
        private static LIConfig _configFile = new();

        /// <summary>
        /// Gets the current directory where config file is stored.
        /// Usually in a LevelImposter folder beside the LevelImposter.dll.
        /// </summary>
        /// <returns>String path where config file is stored.</returns>
        public static string GetDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            return Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", "LevelImposter/config.json");
        }

        /// <summary>
        /// Reads all data from the configuration file
        /// </summary>
        public static void ReadAll()
        {
            string directory = GetDirectory();
            if (!File.Exists(directory))
                return;
            string configJSON = File.ReadAllText(directory);
            _configFile = JsonSerializer.Deserialize<LIConfig>(configJSON) ?? new();
        }

        /// <summary>
        /// Saves all data to the configuration file.
        /// This is not done automatically!
        /// </summary>
        public static void Save()
        {
            try
            {
                LILogger.Info("Saving local config file");
                string path = GetDirectory();
                string directory = Path.GetDirectoryName(path) ?? path;

                // Create Directory
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Write File
                string configJSON = JsonSerializer.Serialize(_configFile);
                File.WriteAllText(path, configJSON);
            }
            catch (System.Exception e)
            {
                LILogger.Error("Failed to save local config file");
                LILogger.Info(e);
            }
        }

        /// <summary>
        /// Gets the random weight of a map
        /// </summary>
        /// <param name="mapID">Map ID to get weight of</param>
        /// <returns>Value from 0 to 1</returns>
        public static float GetMapWeight(string mapID)
        {
            if (_configFile.RandomWeights?.ContainsKey(mapID) == true)
                return _configFile.RandomWeights[mapID];
            return 1.0f;
        }

        /// <summary>
        /// Sets the random weight of a map
        /// </summary>
        /// <param name="mapID">Map ID to set the weight of</param>
        /// <param name="weight">Value from 0 to 1</param>
        public static void SetMapWeight(string mapID, float weight)
        {
            if (_configFile.RandomWeights == null)
                _configFile.RandomWeights = new();
            if (_configFile.RandomWeights.ContainsKey(mapID))
                _configFile.RandomWeights[mapID] = weight;
            else
                _configFile.RandomWeights.Add(mapID, weight);
        }

        /// <summary>
        /// Gets the Map ID of the last opened map
        /// </summary>
        /// <returns>Map ID or null if none used</returns>
        public static string? GetLastMapID()
        {
            return _configFile.LastMapJoined;
        }

        /// <summary>
        /// Sets the Map ID of the last opened map
        /// </summary>
        /// <param name="mapID">Map ID or null if none used</param>
        public static void SetLastMapID(string? mapID)
        {
            if (_configFile.LastMapJoined == mapID)
                return;
            _configFile.LastMapJoined = mapID;
            Save();
        }
    }
}