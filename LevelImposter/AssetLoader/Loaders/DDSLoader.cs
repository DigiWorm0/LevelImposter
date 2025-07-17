using System;
using System.IO;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class DDSLoader
{
    private const int DDS_HEADER_SIZE = 128;
    private const int DDS_PIXEL_FORMAT_SIZE = 32;

    /// <summary>
    ///     Loads a DDS (DirectDraw Surface) image from a stream.
    /// </summary>
    /// <param name="imgStream">Raw DDS file stream</param>
    /// <param name="loadable">Sprite options to apply</param>
    /// <returns>A still UnityEngine.Sprite containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static LoadedSprite Load(Stream imgStream, LoadableSprite loadable)
    {
        // Get All Data
        var imageDataBuffer = new byte[imgStream.Length];
        var readBytes = imgStream.Read(imageDataBuffer, 0, imageDataBuffer.Length);
        if (readBytes != imageDataBuffer.Length)
            throw new IOException("Failed to read all image data");

        // Get Options
        var options = loadable.Options;

        // Create Texture
        var sprite = ImageDataToSprite(
            imageDataBuffer,
            loadable.ID,
            options.Pivot,
            options.PixelArt
        );

        // Register in GC
        GCHandler.Register(sprite);

        // Create Loaded Sprite
        return new LoadedSprite(sprite);
    }

    /// <summary>
    ///     Reads a 32-bit unsigned integer from a byte array at the specified offset.
    /// </summary>
    /// <param name="data">The byte array containing the data.</param>
    /// <param name="offset">The offset in the byte array where the integer starts.</param>
    /// <returns>The 32-bit unsigned integer read from the byte array.</returns>
    private static int ReadDword(byte[] data, int offset)
    {
        return data[offset] |
               (data[offset + 1] << 8) |
               (data[offset + 2] << 16) |
               (data[offset + 3] << 24);
    }

    /// <summary>
    ///     Converts a four-character code (FourCC) to a Unity TextureFormat.
    /// </summary>
    /// <returns>Unity TextureFormat corresponding to the FourCC code.</returns>
    private static TextureFormat CharacterCodeToTextureFormat(int fourCharacterCode)
    {
        return fourCharacterCode switch
        {
            0x31545844 => TextureFormat.DXT1,
            0x35545844 => TextureFormat.DXT5,
            _ => throw new NotSupportedException("Unsupported FourCC code: " + fourCharacterCode)
        };
    }

    /// <summary>
    ///     Checks if a stream matches a DDS (DirectDraw Surface) file format.
    ///     Resets the stream position to the beginning after checking.
    /// </summary>
    /// <param name="dataStream">The stream to check for DDS format.</param>
    /// <returns>True if the stream is a valid DDS file, otherwise false.</returns>
    public static bool IsDDS(Stream dataStream)
    {
        using (var reader = new BinaryReader(dataStream, Encoding.ASCII, true))
        {
            try
            {
                // Read Header
                var header = reader.ReadBytes(4);
                if (header.Length != 4)
                    return false;
                reader.BaseStream.Position = 0;

                // Check for DDS magic number
                return header[0] == 'D' && header[1] == 'D' && header[2] == 'S' && header[3] == ' ';
            }

            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    ///     Converts raw DDS (DirectDraw Surface) bytes to a still sprite.
    ///     <para>
    ///         This is a relatively expensive operation and must be done on the main Unity thread.
    ///         Texture data is removed from CPU memory making the resulting Sprite non-readable.
    ///     </para>
    /// </summary>
    /// <param name="imgData">Raw DDS data in within IL2CPP memory</param>
    /// <param name="name">Name of the resulting sprite objects</param>
    /// <param name="pivot">Pivots the sprite by a Vector2. (Default: 0.5f, 0.5f)</param>
    /// <param name="isPixelArt">Whether the image is pixel art or not. Disables Bilinear filtering. (Default: false)</param>
    /// <returns>A Unity Sprite containing the resulting image data</returns>
    [HideFromIl2Cpp]
    public static Sprite ImageDataToSprite(
        byte[] imgData,
        string name = "CustomSprite",
        Vector2? pivot = null,
        bool isPixelArt = false)
    {
        // Check the first 4 bytes for DDS magic number
        if (imgData.Length < 4 ||
            imgData[0] != 'D' ||
            imgData[1] != 'D' ||
            imgData[2] != 'S' ||
            imgData[3] != ' ')
            throw new Exception("Invalid DDS texture. Unable to read");

        // Check if the header size is correct
        var headerSize = ReadDword(imgData, 4);
        if (headerSize != DDS_HEADER_SIZE - 4) // Subtract 4 for storing the size itself
            throw new Exception("Invalid DDS header size. Expected 124 bytes.");

        // DDS Header Fields
        // var flags = ReadDword(imgData, 8);
        var imgHeight = ReadDword(imgData, 12);
        var imgWidth = ReadDword(imgData, 16);
        // var pitchOrLinearSize = ReadDword(imgData, 20);
        // var depth = ReadDword(imgData, 24);
        var mipMapCount = ReadDword(imgData, 28);

        // Pixel Format
        var pixelFormatSize = ReadDword(imgData, 76);
        if (pixelFormatSize != DDS_PIXEL_FORMAT_SIZE)
            throw new Exception("Invalid DDS pixel format size. Expected 32 bytes.");

        // var pixelFormatFlags = ReadDword(imgData, 80);
        var fourCharacterCode = ReadDword(imgData, 84);
        // var rgbBitCount = ReadDword(imgData, 88);
        // var rBitMask = ReadDword(imgData, 92);
        // var gBitMask = ReadDword(imgData, 96);
        // var bBitMask = ReadDword(imgData, 100);
        // var aBitMask = ReadDword(imgData, 104);

        // DDS Capabilities
        // var caps1 = ReadDword(imgData, 108);
        // var caps2 = ReadDword(imgData, 112);

        // Strip DDS header from DXT texture data
        var dxtTextureData = new byte[imgData.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(
            imgData,
            DDS_HEADER_SIZE,
            dxtTextureData,
            0,
            imgData.Length - DDS_HEADER_SIZE);

        // Build texture from DXT data
        var textureFormat = CharacterCodeToTextureFormat(fourCharacterCode);
        var texture = new Texture2D(
            imgWidth,
            imgHeight,
            textureFormat,
            mipMapCount > 1)
        {
            name = $"{name}_tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = isPixelArt ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };
        texture.LoadRawTextureData(dxtTextureData);

        // Remove from CPU Memory
        texture.Apply(false, true);

        // Generate Sprite
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, imgWidth, imgHeight),
            pivot ?? new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );

        // Set Sprite Flags
        sprite.name = $"{name}_sprite";
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;

        return sprite;
    }
}