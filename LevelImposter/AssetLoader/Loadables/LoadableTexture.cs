using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public readonly struct LoadableTexture(string _id, IStreamable _streamable) : ICachable
{
    public string ID => _id;
    public IStreamable Streamable => _streamable;
    public readonly TextureOptions Options { get; } = new();

    public class TextureOptions
    {
        public bool PixelArt { get; set; } = false;
    }
}