using System.Threading.Tasks;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.AssetLoader.Loaders;
using LevelImposter.AssetLoader.Queue;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Test;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class SpriteLoader : AssetLoader<SpriteInfo, SpriteResult>
{
    private SpriteLoader()
    {
    }

    public static SpriteLoader Instance { get; } = new();

    protected override async Task<SpriteResult> LoadAsset(SpriteInfo loadable)
    {
        using var _ = Profiler.Measure("SpriteLoader.Load", loadable.ID);

        // Load the texture
        var loadedTexture = await TextureLoader.Instance.Load(loadable.TextureInfo);

        // Generate Sprite
        return await UnityThreadQueue.Run(() => BuildSprite(loadable, loadedTexture));
    }

    private static SpriteResult BuildSprite(SpriteInfo loadable, TextureResult loadedTexture)
    {
        UnityThreadQueue.AssertMainThread("SpriteLoader.BuildSprite");

        // If this is a GIF, we can save time/memory by using the Sprite that the GIFLoader already generated for us.
        if (loadedTexture is GIFLoader.GifTextureResult gifResult)
        {
            var firstFrameSprite = gifResult.GIFFile.GetFrameSprite(0);
            return new SpriteResult(firstFrameSprite, loadedTexture);
        }

        var options = loadable.Options;
        var texture = loadedTexture.Texture;

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
        GCHandler.Register(sprite, options.GCBehavior);

        // Return Loaded Sprite
        return new SpriteResult(sprite, loadedTexture);
    }
}