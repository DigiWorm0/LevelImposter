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
        // Load data into memory
        var data = loadable.DataStore.LoadToMemory();
        
        // Determine file type
        var fileType = GetFileType(data);
        
        // Load the sprite
        var loadedTexture = fileType switch
        {
            FileType.DDS => DDSLoader.Load(loadable, data),
            FileType.GIF => GIFLoader.Load(loadable, data),
            _ => PNGLoader.Load(loadable, data)
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
    /// <param name="data">Data block to check</param>
    /// <returns>Cooresponding file type</returns>
    private static FileType? GetFileType(MemoryBlock data)
    {
        if (GIFFile.IsGIF(data))
            return FileType.GIF;
        if (DDSLoader.IsDDS(data))
            return FileType.DDS;
        
        return null;
    }
}