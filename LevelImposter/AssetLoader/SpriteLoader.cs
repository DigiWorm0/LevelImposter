using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class SpriteLoader : AsyncQueue<SpriteInfo, SpriteResult>
{
    private SpriteLoader()
    {
    }

    public static SpriteLoader Instance { get; } = new();

    protected override SpriteResult Load(SpriteInfo loadable)
    {
        // Load the texture
        var loadedTexture = TextureLoader.Instance.LoadImmediate(loadable.TextureInfo);
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
        GCHandler.Register(sprite, options.GCBehavior);

        // Return Loaded Sprite
        return new SpriteResult(sprite, loadedTexture);
    }
}