using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public static class DDSLoader
{
    /// <summary>
    /// Unity only supports reading the texture data starting from byte offset 128.
    /// Data before this is the DDS header.
    /// </summary>
    private const int DDS_TEXTURE_OFFSET = 128;
    private const int DDS_PIXEL_FORMAT_SIZE = 32;

    /// <summary>
    ///     Loads a DDS (DirectDraw Surface) image from a loadable.
    /// </summary>
    /// <param name="loadable">Loadable texture</param>
    /// <returns>A still UnityEngine.Texture2D containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static LoadedTexture Load(LoadableTexture loadable)
    {
        // Rent buffers from the pool
        var imgData = loadable.DataStore.LoadToMemory();

        // Create Texture
        var texture = ImageDataToTexture2D(
            imgData,
            loadable.ID,
            loadable.Options
        );

        // Return the created texture
        return new LoadedTexture(texture);
    }

    /// <summary>
    ///     Reads a 32-bit unsigned integer from a byte array at the specified offset.
    /// </summary>
    /// <param name="data">The byte array containing the data.</param>
    /// <param name="offset">The offset in the byte array where the integer starts.</param>
    /// <returns>The 32-bit unsigned integer read from the byte array.</returns>
    private static int ReadDword(MemoryBlock data, int offset)
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
    ///     Checks if the given data is a DDS file.
    /// </summary>
    /// <param name="data">MemoryBlock of raw DDS data</param>
    /// <returns>True if the data is a DDS file. False otherwise</returns>
    public static bool IsDDS(byte[] data)
    {
        return data[0] == 'D' &&
               data[1] == 'D' &&
               data[2] == 'S' &&
               data[3] == ' ';
    }

    /// <summary>
    ///     Converts raw DDS (DirectDraw Surface) bytes to a still Texture2D.
    ///     <para>
    ///         This is a relatively expensive operation and must be done on the main Unity thread.
    ///         Texture data is removed from CPU memory making the resulting texture non-readable.
    ///     </para>
    /// </summary>
    /// <param name="textureData">Raw texture data in within memory</param>
    /// <param name="name">Name of the resulting texture object</param>
    /// <param name="options">Texture options to apply</param>
    /// <returns>A Unity Texture2D containing the resulting image data</returns>
    [HideFromIl2Cpp]
    private static Texture2D ImageDataToTexture2D(
        MemoryBlock textureData,
        string name = "CustomTexture",
        LoadableTexture.TextureOptions? options = null)
    {
        // Check the first 4 bytes for DDS magic number
        if (textureData.Data.Length < 4 ||
            textureData.Data[0] != 'D' ||
            textureData.Data[1] != 'D' ||
            textureData.Data[2] != 'S' ||
            textureData.Data[3] != ' ')
            throw new Exception("Invalid DDS texture. Unable to read");

        // Check if the header size is correct
        var headerSize = ReadDword(textureData, 4);
        if (headerSize != 124) // Subtract 4 for storing the size itself
            throw new Exception("Invalid DDS header size. Expected 124 bytes.");

        // DDS Header Fields
        var imgHeight = ReadDword(textureData, 12);
        var imgWidth = ReadDword(textureData, 16);
        var mipMapCount = ReadDword(textureData, 28);

        // Pixel Format
        var pixelFormatSize = ReadDword(textureData, 76);
        if (pixelFormatSize != DDS_PIXEL_FORMAT_SIZE)
            throw new Exception("Invalid DDS pixel format size. Expected 32 bytes.");

        // FourCC Code
        var fourCharacterCode = ReadDword(textureData, 84);

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
            filterMode = options?.PixelArt ?? false ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };
    
        // Make sure pointer is offset by 128 bytes to skip DDS header
        var textureDataPtr = IntPtr.Add(textureData.BasePointer, DDS_TEXTURE_OFFSET);
        var textureDataSize = textureData.Length - DDS_TEXTURE_OFFSET;
        
        // Double-check before casting size to int (should never happen)
        if (textureDataSize > int.MaxValue)
            throw new Exception("DDS texture data is too large to load.");
        
        // Load texture data from pointer
        texture.LoadRawTextureData(textureDataPtr, (int)textureDataSize);

        // Remove texture data from CPU memory
        texture.Apply(false, true);
        
        // Register in GC
        if (options?.AddToGC ?? true)
            GCHandler.Register(texture);
        
        return texture;
    }
}