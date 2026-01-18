using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableSprite(string id, LoadableTexture tex) : IIdentifiable
{
    public string ID => id;
    public LoadableTexture Texture => tex;
    public SpriteOptions Options { get; } = new();

    public class SpriteOptions
    {
        /// If set, defines the pivot point of the sprite. Otherwise, the center (0.5, 0.5) is used.
        public Vector2? Pivot { get; set; }
        
        /// If set, defines the portion of the texture to use for the sprite. Otherwise, the full texture is used.
        public Rect? Frame { get; set; }
        
        /// Changes how and when the texture is disposed of. 
        /// <c>null</c> will use <see cref="GCHandler"/>'s current default behavior.
        public GCBehavior? GCBehavior { get; set; } = null;
    }

    /// <summary>
    /// Creates a LoadableSprite from a LoadableTexture.
    /// Copies the ID and uses the provided texture.
    /// </summary>
    /// <param name="texture">LoadableTexture to create the sprite from.</param>
    /// <returns>A LoadableSprite instance.</returns>
    public static LoadableSprite FromLoadableTexture(LoadableTexture texture)
    {
        var loadableSprite = new LoadableSprite(texture.ID, texture);
        loadableSprite.Options.GCBehavior = texture.Options.GCBehavior;
        return loadableSprite;
    }
}