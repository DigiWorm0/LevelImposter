using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableSprite(string _id, IStreamable _streamable) : ICachable
{
    public string ID => _id;
    public IStreamable Streamable => _streamable;
    public readonly SpriteOptions Options { get; } = new();

    public class SpriteOptions
    {
        public Vector2 Pivot { get; set; } = new(0.5f, 0.5f);
        public bool PixelArt { get; set; } = false;
    }
}