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
        public Vector2? Pivot { get; set; }
        public Rect? Frame { get; set; }
    }
}