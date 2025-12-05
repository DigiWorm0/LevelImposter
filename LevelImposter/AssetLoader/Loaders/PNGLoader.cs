using System.Buffers;
using System.IO;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public static class PNGLoader
{
    /// <summary>
    ///     We must read the entire file into memory to process it.
    ///     We can use a shared ArrayPool to avoid excessive memory allocations.
    /// </summary>
    private static ArrayPool<byte> BytePool => ArrayPool<byte>.Shared;

    /// <summary>
    ///     Loads a PNG/JPG image from a stream.
    /// </summary>
    /// <param name="imgStream">Raw PNG/JPG file stream</param>
    /// <param name="loadable">Texture options to apply</param>
    /// <returns>A still UnityEngine.Texture2D containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static LoadedTexture Load(Stream imgStream, LoadableTexture loadable)
    {
        // Before we do anything, rent a buffer for reading the image data
        var imageDataLength = (int)imgStream.Length;
        var imageDataBuffer = BytePool.Rent(imageDataLength);

        try
        {
            // Read Image Data
            var readBytes = imgStream.Read(imageDataBuffer, 0, imageDataLength);
            if (readBytes != imageDataLength)
                throw new IOException("Failed to read all image data");

            // Get Options
            var options = loadable.Options;

            // Create Texture
            var texture = ImageDataToTexture2D(
                imageDataBuffer,
                loadable.ID,
                options
            );

            // Return the rented buffer
            BytePool.Return(imageDataBuffer);
            
            // Return the loaded texture
            return new LoadedTexture(texture);
        }
        catch
        {
            // If we fail, we must return the rented buffer
            BytePool.Return(imageDataBuffer);
            
            // Rethrow the exception
            throw;
        }
    }

    /// <summary>
    ///     Converts raw PNG/JPG bytes to a still texture.
    ///     <para>
    ///         This is a relatively expensive operation and must be done on the main Unity thread.
    ///         Texture data is removed from CPU memory making the resulting texture non-readable.
    ///     </para>
    /// </summary>
    /// <param name="imgData">Raw PNG/JPG data in within IL2CPP memory</param>
    /// <param name="name">Name of the resulting texture objects</param>
    /// <param name="options">Texture options to apply</param>
    /// <returns>A Unity Texture2D containing the resulting image data</returns>
    [HideFromIl2Cpp]
    public static Texture2D ImageDataToTexture2D(
        byte[] imgData,
        string name = "CustomTexture",
        LoadableTexture.TextureOptions? options = null)
    {
        // Generate Texture
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
        {
            name = $"{name}_tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = (options?.PixelArt ?? false) ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };
        texture.LoadImage(imgData);

        // Remove from CPU Memory
        texture.Apply(false, true);

        // Add to GC
        if (options?.AddToGC ?? true)
            GCHandler.Register(texture);
        
        // Return Texture
        return texture;
    }
}