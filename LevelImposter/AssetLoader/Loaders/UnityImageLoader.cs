using System.IO;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.FileIO.DataBlock;
using LevelImposter.Test;
using UnityEngine;

namespace LevelImposter.AssetLoader.Loaders;

public static class UnityImageLoader
{
    /// <summary>
    ///     Loads a PNG/JPG image from a stream.
    /// </summary>
    /// <param name="loadable">Texture options to apply</param>
    /// <returns>A still UnityEngine.Texture2D containing the image data</returns>
    /// <exception cref="IOException">If the Stream fails to read image data</exception>
    public static async Task<TextureResult> Load(TextureInfo loadable)
    {
        using var _ = Profiler.Measure("PNGLoader.Load", loadable.ID);

        // Create Texture
        var texture = await UnityThreadQueue.Run(() => ImageDataToTexture2D(
            loadable.DataStore.LoadToMemory(),
            loadable.ID,
            loadable.Options
        ));

        // Return the loaded texture
        return new TextureResult(texture);
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
        MemoryBlock imgData,
        string name = "CustomTexture",
        TextureInfo.TextureOptions? options = null)
    {
        // Generate Texture
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
        {
            name = $"{name}_tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = options?.PixelArt ?? false ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };
        texture.LoadImage(imgData.Data, true);

        // Add to GC
        GCHandler.Register(texture, options?.GCBehavior);

        // Return Texture
        return texture;
    }
}