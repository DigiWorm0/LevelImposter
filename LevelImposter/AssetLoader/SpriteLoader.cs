using System;
using System.IO.Compression;
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
    /// <param name="id">ID of the sprite</param>
    /// <param name="dataStore">Data store to load from</param>
    /// <param name="onLoad">Callback when the sprite is loaded</param>
    public static void LoadAsync(string id, IDataStore dataStore, Action<Sprite> onLoad)
    {
        var loadableTexture = new LoadableTexture(id, dataStore);
        var loadableSprite = new LoadableSprite(id, loadableTexture);
        Instance.AddToQueue(loadableSprite, loadedSprite => onLoad(loadedSprite));
    }

    /// <summary>
    ///    Simplified shorthand to load a sprite synchronously.
    /// </summary>
    /// <param name="sprite">Loadable sprite data</param>
    /// <returns>Loaded sprite</returns>
    public static LoadedSprite LoadSync(LoadableSprite sprite)
    {
        return Instance.LoadImmediate(sprite);
    }

    protected override LoadedSprite Load(LoadableSprite loadable)
    {
        // Load the texture
        var loadedTexture = TextureLoader.LoadSync(loadable.Texture);
        var texture = loadedTexture.Texture;
        
        // Generate Sprite
        var options = loadable.Options;
        var sprite = Sprite.Create(
            texture,
            options.Frame ?? new Rect(0, 0, texture.width, texture.height),
            options.Pivot ?? new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );

        // Set Sprite Flags
        sprite.name = $"{loadable.ID}_sprite";
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        
        // Register in GC
        if (options.AddToGC)
            GCHandler.Register(sprite);
        
        // Return Loaded Sprite
        return new LoadedSprite(sprite, loadedTexture);
    }
}