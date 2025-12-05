using System;
using System.Buffers;
using System.IO;
using System.Text;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public static class DDSLoader
{
    private const int DDS_HEADER_SIZE = 128;
    private const int DDS_PIXEL_FORMAT_SIZE = 32;

    /// <summary>
    ///     We must read the entire file into memory to process it.
    ///     We can use a shared ArrayPool to avoid excessive memory allocations.
    /// </summary>
    private static ArrayPool<byte> BytePool => ArrayPool<byte>.Shared;

    /// <summary>
    ///     Loads a DDS (DirectDraw Surface) image from a loadable.
    /// </summary>
    /// <param name="imgStream">Raw DDS file stream</param>
    /// <param name="loadable">Loadable texture</param>
    /// <returns>A still UnityEngine.Texture2D containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static LoadedTexture Load(Stream imgStream, LoadableTexture loadable)
    {
        // Before we do anything, rent buffers for reading the DDS header and texture data
        var textureDataLength = (int)imgStream.Length - DDS_HEADER_SIZE;
        var headerData = BytePool.Rent(DDS_HEADER_SIZE);
        var imageDataBuffer = BytePool.Rent(textureDataLength);

        // Ensure we return the rented buffers in case of an exception
        try
        {
            // Read DDS Header
            var readBytes = imgStream.Read(headerData, 0, DDS_HEADER_SIZE);
            if (readBytes != DDS_HEADER_SIZE)
                throw new IOException("Failed to read DDS header data");

            // Read Texture Data
            readBytes = imgStream.Read(imageDataBuffer, 0, textureDataLength);
            if (readBytes != textureDataLength)
                throw new IOException("Failed to read all image data");

            // Create Texture
            var texture = ImageDataToTexture2D(
                headerData,
                imageDataBuffer,
                loadable.ID,
                loadable.Options
            );

            // Return the rented buffers
            BytePool.Return(headerData);
            BytePool.Return(imageDataBuffer);

            // Return the created texture
            return new LoadedTexture(texture);
        }
        catch
        {
            // If any exception occurs, return the rented buffers to the pool
            BytePool.Return(headerData);
            BytePool.Return(imageDataBuffer);

            // Re-throw the exception to be handled by the caller
            throw;
        }
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
        using var reader = new BinaryReader(dataStream, Encoding.ASCII, true);
        
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

    /// <summary>
    ///     Converts raw DDS (DirectDraw Surface) bytes to a still Texture2D.
    ///     <para>
    ///         This is a relatively expensive operation and must be done on the main Unity thread.
    ///         Texture data is removed from CPU memory making the resulting texture non-readable.
    ///     </para>
    /// </summary>
    /// <param name="headerData">DDS header data in within IL2CPP memory</param>
    /// <param name="textureData">Raw texture data in within IL2CPP memory</param>
    /// <param name="name">Name of the resulting texture object</param>
    /// <param name="options">Texture options to apply</param>
    /// <returns>A Unity Texture2D containing the resulting image data</returns>
    [HideFromIl2Cpp]
    private static Texture2D ImageDataToTexture2D(
        byte[] headerData,
        byte[] textureData,
        string name = "CustomTexture",
        LoadableTexture.TextureOptions? options = null)
    {
        // Check the first 4 bytes for DDS magic number
        if (headerData.Length < 4 ||
            headerData[0] != 'D' ||
            headerData[1] != 'D' ||
            headerData[2] != 'S' ||
            headerData[3] != ' ')
            throw new Exception("Invalid DDS texture. Unable to read");

        // Check if the header size is correct
        var headerSize = ReadDword(headerData, 4);
        if (headerSize != DDS_HEADER_SIZE - 4) // Subtract 4 for storing the size itself
            throw new Exception("Invalid DDS header size. Expected 124 bytes.");

        // DDS Header Fields
        var imgHeight = ReadDword(headerData, 12);
        var imgWidth = ReadDword(headerData, 16);
        var mipMapCount = ReadDword(headerData, 28);

        // Pixel Format
        var pixelFormatSize = ReadDword(headerData, 76);
        if (pixelFormatSize != DDS_PIXEL_FORMAT_SIZE)
            throw new Exception("Invalid DDS pixel format size. Expected 32 bytes.");

        // FourCC Code
        var fourCharacterCode = ReadDword(headerData, 84);

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
        texture.LoadRawTextureData(textureData);

        // Remove from CPU Memory
        texture.Apply(false, true);
        
        // Register in GC
        if (options?.AddToGC ?? true)
            GCHandler.Register(texture);
        
        return texture;
    }
}