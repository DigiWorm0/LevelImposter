using LevelImposter.Core;
using System;
using System.IO;
using System.Text.Json;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Converts LIM to LIM2 files
    /// </summary>
    public static class LegacyConverter
    {
        public static string GetLegacyPath(string mapID)
        {
            return Path.Combine(MapFileAPI.GetDirectory(), $"{mapID}.lim");
        }

        public static bool ConvertAllFiles()
        {
            bool hasConverted = false;
            string[] legacyMapIDs = Directory.GetFiles(MapFileAPI.GetDirectory(), "*.lim");
            foreach (string legacyMapID in legacyMapIDs)
            {
                string mapID = Path.GetFileNameWithoutExtension(legacyMapID);
                if (!MapFileAPI.Exists(mapID))
                {
                    ConvertFile(mapID);
                    hasConverted = true;
                }
            }
            return hasConverted;
        }

        public static void UpdateMap(LIMap map)
        {
            if (!map.isLegacy)
                return;

            // Make SpriteDB
            map.spriteDB = new();
            foreach (LIElement element in map.elements)
            {
                // Add Sprite Data
                if (element.properties.spriteData != null)
                {
                    Guid spriteID = Guid.NewGuid();
                    map.spriteDB.Add(spriteID, element.properties.spriteData);
                    element.properties.spriteID = spriteID;
                    element.properties.spriteData = null;
                }

                // TODO: Add Other Lengthy Data
            }
        }

        public static void ConvertFile(string mapID)
        {
            LILogger.Info($"Converting legacy map file [{mapID}]");

            // Get paths
            string legacyPath = GetLegacyPath(mapID);
            string newPath = MapFileAPI.GetPath(mapID);

            // Check if files exist
            if (!File.Exists(legacyPath))
                throw new FileNotFoundException($"Could not find legacy map file {legacyPath}");
            if (File.Exists(newPath))
                throw new FileLoadException($"Map file {newPath} already exists");

            // Read legacy file
            LIMap? mapFile;
            using (FileStream legacyFileStream = File.OpenRead(legacyPath))
                mapFile = JsonSerializer.Deserialize<LIMap>(legacyFileStream);

            // Check if file is valid
            if (mapFile == null)
                throw new FileLoadException($"Could not deserialize legacy map file {legacyPath}");

            // Update map
            UpdateMap(mapFile);

            // Serialize
            string newFileData = LISerializer.SerializeMap(mapFile);

            // Write new file
            using (FileStream newFileStream = File.OpenWrite(newPath))
            using (StreamWriter newFileWriter = new(newFileStream))
                newFileWriter.Write(newFileData);

            // Delete legacy file
            //File.Delete(legacyPath); // <-- We'll probably want to keep legacy files in case of failure
        }
    }
}
