using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LevelImposter.Core;

namespace LevelImposter.Shop;

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
            if (_options == null)
            {
                _options = new JsonSerializerOptions();
                _options.DefaultIgnoreCondition =
                    JsonIgnoreCondition.WhenWritingNull;
            }

            // Update Legacy Format
            if (mapData.isLegacy)
                LegacyConverter.UpdateMap(mapData);

            // Map Data
            var mapJsonBytes = JsonSerializer.SerializeToUtf8Bytes(mapData, _options);
            stream.Write(BitConverter.GetBytes(mapJsonBytes.Length));
            stream.Write(mapJsonBytes);

            // SpriteDB
            if (mapData.mapAssetDB != null)
                foreach (var spriteAsset in mapData.mapAssetDB.DB)
                {
                    var data = spriteAsset.Value.LoadToMemory().Data;
                    var idBytes = Encoding.UTF8.GetBytes(spriteAsset.Key.ToString());

                    // Write Element
                    stream.Write(idBytes);
                    stream.Write(BitConverter.GetBytes(data.Length)); // <-- TODO: ** Improve memory usage here **
                    stream.Write(data);
                }
        }
        catch (Exception ex)
        {
            LILogger.Error(ex);
        }
    }
}