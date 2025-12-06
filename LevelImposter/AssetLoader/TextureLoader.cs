using System;
using System.Buffers;
using System.IO;
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
    /// Checks the file type of loadable texture by
    /// inspecting its magic numbers from a data stream.
    /// </summary>
    /// <param name="loadable">Loadable texture</param>
    /// <returns>Cooresponding file type</returns>
    private static FileType? GetFileType(LoadableTexture loadable)
    {
        // Buffer the first 16 bytes of the stream
        // This is because we can't seek some types of streams
        using var loadableStream = loadable.DataStore.OpenStream();
        using var memoryBlock = new PoolableMemoryBlock(FILE_TYPE_BUFFER_SIZE);
        loadableStream.Read(memoryBlock.Get(), 0, FILE_TYPE_BUFFER_SIZE);
        
        // Check file type
        using var memoryStream = new MemoryStream(memoryBlock.Get());
        if (GIFFile.IsGIF(memoryStream))
            return FileType.GIF;
        if (DDSLoader.IsDDS(memoryStream))
            return FileType.DDS;
        
        return null;
    }
}