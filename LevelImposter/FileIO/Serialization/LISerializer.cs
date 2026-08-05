using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;

namespace LevelImposter.FileIO.Serialization;

public static class LISerializer
{
    private static JsonSerializerOptions? _options;

    /// <summary>
    ///     Serializes a map into a string
    /// </summary>
    /// <param name="mapData">Map Data to serialize</param>
    /// <param name="stream">Stream to write to</param>
    public static void SerializeMap(LIMap mapData, Stream stream)
    {
        try
        {
            // Create Options
            _options ??= new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Map Data
            var mapJsonBytes = JsonSerializer.SerializeToUtf8Bytes(mapData, _options);
            stream.Write("LIM2"u8);
            stream.Write(BitConverter.GetBytes(mapJsonBytes.Length));
            stream.Write(mapJsonBytes);

            // SpriteDB
            if (mapData.MapAssetDB == null)
                return;

            foreach (var mapAsset in mapData.MapAssetDB.DB)
            {
                // Load Asset to Memory
                var data = mapAsset.Value.LoadToManagedMemory();
                var idBytes = Encoding.UTF8.GetBytes(mapAsset.Key.ToString());

                // Write Asset to Stream
                stream.Write(idBytes);
                stream.Write(BitConverter.GetBytes(data.Length));
                stream.Write(data);
            }
        }
        catch (Exception ex)
        {
            LILogger.Error($"Error serializing map data: {ex.Message}");
        }
    }
}