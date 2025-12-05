using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableSprite(string _id, LoadableTexture _tex) : ICachable
{
    public string ID => _id;
    public LoadableTexture Texture => _tex;
    public readonly SpriteOptions Options { get; } = new();

    public class SpriteOptions
    {
        /// If set, defines the pivot point of the sprite. Otherwise, the center (0.5, 0.5) is used.
        public Vector2? Pivot { get; set; }
        
        /// If set, defines the portion of the texture to use for the sprite. Otherwise, the full texture is used.
        public Rect? Frame { get; set; }
        
        /// If false, the texture will be disposed automatically after the map is unloaded
        public bool AddToGC { get; set; } = true;
    }

    /// <summary>
    /// Creates a LoadableSprite from a LoadableTexture.
    /// Copies the ID and uses the provided texture.
    /// </summary>
    /// <param name="texture">LoadableTexture to create the sprite from.</param>
    /// <returns>A LoadableSprite instance.</returns>
    public static LoadableSprite FromLoadableTexture(LoadableTexture texture)
    {
        return new LoadableSprite(texture.ID, texture);
    }
}