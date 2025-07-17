using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class SpriteLoader : AsyncQueue<LoadableSprite, LoadedSprite>
{
    private SpriteLoader()
    {
    }

    public static SpriteLoader Instance { get; } = new();

    /// <summary>
    ///     Simplified shorthand to load a sprite asynchronously.
    /// </summary>
    /// <param name="id">ID for cache</param>
    /// <param name="streamable">Streamable with raw image data</param>
    /// <param name="callback">Callback when the sprite is loaded</param>
    public static void LoadAsync(string id, IStreamable streamable, Action<Sprite> callback)
    {
        Instance.AddToQueue(
            new LoadableSprite(id, streamable),
            loadedSprite => callback(loadedSprite.Sprite)
        );
    }

    protected override LoadedSprite Load(LoadableSprite loadable)
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
        var loadedSprite = format switch
        {
            "DDS" => DDSLoader.Load(stream, loadable),
            "GIF" => GIFLoader.Load(stream, loadable),
            _ => PNGLoader.Load(stream, loadable)
        };

        // Return the loaded sprite
        return loadedSprite;
    }
}