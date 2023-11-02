using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using System;
using System.Collections;
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

        /// <summary>
        /// Auto-Updates all downloaded maps
        /// </summary>
        /// <returns></returns>
        [HideFromIl2Cpp]
        public static IEnumerator ConvertAllMaps()
        {
            {
                string[] legacyMapIDs = Directory.GetFiles(MapFileAPI.GetDirectory(), "*.lim");
                foreach (string legacyMapID in legacyMapIDs)
                {
                    string mapID = Path.GetFileNameWithoutExtension(legacyMapID);
                    if (!MapFileAPI.Exists(mapID))
                    {
                        ShopManager.Instance?.SetOverlayEnabled(true);
                        ShopManager.Instance?.SetOverlayText($"Converting legacy maps...\n<size=2>{mapID}");
                        yield return null;
                        ConvertFile(mapID);
                    }
                }
                ShopManager.Instance?.SetOverlayEnabled(false);
            }
        }

#pragma warning disable CS0618 // Handles legacy properties
        /// <summary>
        /// Updates legacy map data to a LIM2 data
        /// </summary>
        /// <param name="map">Legacy Map Data</param>
        public static void UpdateMap(LIMap map)
        {
            if (!map.isLegacy)
                return;

            LILogger.Info($"Converting legacy map data [{map.id}]");

            // Update Properties
            map.isLegacy = false;
            map.mapAssetDB = new();

            // SpriteDB
            foreach (LIElement element in map.elements)
            {
                // Add Sprite Data
                if (element.properties.spriteData != null)
                {
                    Guid spriteID = Guid.NewGuid();
                    var spriteData = MapUtils.ParseBase64(element.properties.spriteData);
                    map.mapAssetDB.Add(spriteID, spriteData);
                    element.properties.spriteID = spriteID;
                    element.properties.spriteData = null;
                }

                // Add Minigame Data
                if (element.properties.minigames != null)
                {
                    foreach (var minigame in element.properties.minigames)
                    {
                        var spriteData = MapUtils.ParseBase64(minigame.spriteData ?? "");
                        if (spriteData != null)
                        {
                            Guid spriteID = Guid.NewGuid();
                            map.mapAssetDB.Add(spriteID, spriteData);
                            minigame.spriteID = spriteID;
                        }
                        minigame.spriteData = null;
                    }
                }

                // Add Sound Data
                if (element.properties.sounds != null)
                {
                    foreach (var sound in element.properties.sounds)
                    {
                        if (sound.isPreset)
                        {
                            sound.presetID = sound.data;
                        }
                        else
                        {
                            var soundData = MapUtils.ParseBase64(sound.data ?? "");
                            if (soundData != null)
                            {
                                Guid soundID = Guid.NewGuid();
                                map.mapAssetDB.Add(soundID, soundData);
                                sound.dataID = soundID;
                            }
                        }
                        sound.data = null;
                    }
                }

                // TODO: Search for duplicate entries
            }
        }
#pragma warning restore CS0618

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

            // Serialize & Write to new file
            using (var dataStream = LISerializer.SerializeMap(mapFile))
            using (FileStream outputFileStream = File.OpenWrite(newPath))
                dataStream.CopyTo(outputFileStream);

            // Delete legacy file
            File.Move(legacyPath, $"{legacyPath}.bak");
        }
    }
}
