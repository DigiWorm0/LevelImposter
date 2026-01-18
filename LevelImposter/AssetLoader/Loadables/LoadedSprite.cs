using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class LoadedSprite(Sprite sprite, LoadedTexture texture) : ICachable
{
    public Sprite Sprite => sprite;
    public LoadedTexture Texture => texture;
    public bool IsExpired => sprite == null || texture.IsExpired;

    public static implicit operator Sprite(LoadedSprite loadedSprite) => loadedSprite.Sprite;
}