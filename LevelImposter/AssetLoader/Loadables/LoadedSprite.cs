using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class LoadedSprite(Sprite sprite, LoadedTexture texture)
{
    public Sprite Sprite => sprite;
    public LoadedTexture Texture => texture;
    
    public static implicit operator Sprite(LoadedSprite loadedSprite) => loadedSprite.Sprite;
}