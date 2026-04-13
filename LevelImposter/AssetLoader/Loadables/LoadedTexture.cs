using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class LoadedTexture(Texture2D texture) : ICachable
{
    public Texture2D Texture => texture;
    public bool IsExpired => texture == null;
    
    public static implicit operator Texture2D(LoadedTexture loadedTexture) => loadedTexture.Texture;
}