using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class TextureLoader : AsyncQueue<LoadableTexture, LoadedTexture>
{
    private TextureLoader()
    {
    }

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
        // Open the stream
        using var stream = loadable.Streamable.OpenStream();

        // Check file type
        var isGIF = GIFFile.IsGIF(stream);
        var isDDS = DDSLoader.IsDDS(stream);
        var format = isGIF ? "GIF" :
            isDDS ? "DDS" :
            "PNG/JPG";

        // Load the sprite
        var loadedTexture = format switch
        {
            "DDS" => DDSLoader.Load(stream, loadable),
            "GIF" => GIFLoader.Load(stream, loadable),
            _ => PNGLoader.Load(stream, loadable)
        };

        // Return the loaded sprite
        return loadedTexture;
    }
}