using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using LevelImposter.Core;

namespace LevelImposter.FileIO;

public static class LICompressedDeserializer
{
    const string MAP_JSON_ENTRY = "map.json";
    
    /// <summary>
    /// Deserializes a LIMap from a compressed stream.
    /// </summary>
    /// <param name="stream">Raw file stream of a ZIP-compressed LIMap file.</param>
    /// <param name="spriteDB">Whether to load the sprite database.</param>
    /// <param name="filePath">File path for loading SpriteDB</param>
    /// <returns>The deserialized LIMap object.</returns>
    public static LIMap? Deserialize(Stream stream, bool spriteDB = true, string? filePath = null)
    {
        // Open ZIP archive
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, false);
        var jsonEntry = zip.GetEntry(MAP_JSON_ENTRY);
        if (jsonEntry == null)
            return null;

        // Deserialize JSON
        using var jsonStream = jsonEntry.Open();
        var mapData = JsonSerializer.Deserialize<LIMap>(jsonStream);
        if (mapData == null)
            throw new InvalidDataException("Failed to deserialize map JSON data.");
        if (!spriteDB)
            return mapData;
        
        // Check file path
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path must be provided to load sprite database from ZIP entries.");
        
        // Load Asset DB
        mapData.mapAssetDB = new MapAssetDB();
        foreach (var zipEntry in zip.Entries)
        {
            // Skip JSON
            if (zipEntry.Name == MAP_JSON_ENTRY)
                continue;
            
            // Check if name is a GUID
            if (!Guid.TryParse(zipEntry.Name, out var guid))
                continue;
            
            mapData.mapAssetDB.Add(guid, new ZIPEntryStore(filePath, zipEntry.Name));
            
            // Copy to buffer
            // TODO: Stream directly without buffering entire file in memory
            // byte[] buffer;
            // using (var entryStream = zipEntry.Open())
            // using (var ms = new MemoryStream())
            // {
            //     entryStream.CopyTo(ms);
            //     buffer = ms.ToArray();
            // }
            //
            // // Add to Asset DB
            // mapData?.mapAssetDB?.Add(guid, new MemoryStreamable(buffer));
        }
        
        return mapData;
    }
}