using System.Buffers;
using System.IO;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public static class PNGLoader
{
    /// <summary>
    ///     Loads a PNG/JPG image from a stream.
    /// </summary>
    /// <param name="imgStream">Raw PNG/JPG file stream</param>
    /// <param name="loadable">Texture options to apply</param>
    /// <returns>A still UnityEngine.Texture2D containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static LoadedTexture Load(LoadableTexture loadable)
    {
        // Read all image data into memory
        using var imgData = loadable.DataStore.LoadToMemory();
        
        // Get Options
        var options = loadable.Options;

        // Create Texture
        var texture = ImageDataToTexture2D(
            imgData.Get(),
            loadable.ID,
            options
        );
        
        // Return the loaded texture
        return new LoadedTexture(texture);
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
    private static Texture2D ImageDataToTexture2D(
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