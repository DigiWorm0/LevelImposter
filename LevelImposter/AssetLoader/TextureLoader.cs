using System;
using System.Buffers;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class TextureLoader : AsyncQueue<LoadableTexture, LoadedTexture>
{
    private TextureLoader()
    {
    }

    /// <summary>
    /// How many bytes to read from the start of the file
    /// to determine its file type (magic numbers).
    /// </summary>
    private const int FILE_TYPE_BUFFER_SIZE = 8;
    
    public static TextureLoader Instance { get; } = new();
    
    /// <summary>
    /// Loads a texture 2D synchronously.
    /// </summary>
    /// <param name="loadable">Loadable texture 2d</param>
    /// <returns>Loaded texture 2d</returns>
    public static LoadedTexture LoadSync(LoadableTexture loadable)
    {
        return Instance.LoadImmediate(loadable);
    }

    protected override LoadedTexture Load(LoadableTexture loadable)
    {
        // Determine file type
        var fileType = GetFileType(loadable);
        
        // Load the sprite
        var loadedTexture = fileType switch
        {
            FileType.DDS => DDSLoader.Load(loadable),
            FileType.GIF => GIFLoader.Load(loadable),
            _ => PNGLoader.Load(loadable)
        };

        // Return the loaded sprite
        return loadedTexture;
    }
    
    /// <summary>
    /// Enumeration of supported file types.
    /// </summary>
    private enum FileType
    {
        GIF,
        DDS
    }
    
    /// <summary>
    /// Checks the file type of <see cref="MemoryBlock"/> by
    /// inspecting its magic numbers from a data stream.
    /// </summary>
    /// <param name="loadable">Loadable texture to check</param>
    /// <returns>Cooresponding file type</returns>
    private static FileType? GetFileType(LoadableTexture loadable)
    {
        // Peek at the start of the data
        var signature = loadable.DataStore.Peek(FILE_TYPE_BUFFER_SIZE);

        if (GIFFile.IsGIF(signature))
            return FileType.GIF;
        if (DDSLoader.IsDDS(signature))
            return FileType.DDS;
        
        return null;
    }
}