using System.Buffers;
using System.IO;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class PNGLoader
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
    /// <param name="loadable">Sprite options to apply</param>
    /// <returns>A still UnityEngine.Sprite containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static LoadedSprite Load(Stream imgStream, LoadableSprite loadable)
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
        catch
        {
            // If we fail, we must return the rented buffer
            BytePool.Return(imageDataBuffer);
            throw;
        }
    }

    /// <summary>
    ///     Converts raw PNG/JPG bytes to a still sprite.
    ///     <para>
    ///         This is a relatively expensive operation and must be done on the main Unity thread.
    ///         Texture data is removed from CPU memory making the resulting Sprite non-readable.
    ///     </para>
    /// </summary>
    /// <param name="imgData">Raw PNG/JPG data in within IL2CPP memory</param>
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
        // Generate Texture
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
        {
            name = $"{name}_tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = isPixelArt ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };
        texture.LoadImage(imgData);

        // Remove from CPU Memory
        texture.Apply(false, true);

        // Generate Sprite
        var sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
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