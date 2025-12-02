using System;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.AssetLoader;

public class LoadedTexture(Texture2D texture)
{
    public Texture2D Texture => texture;
    
    public static implicit operator Texture2D(LoadedTexture loadedTexture) => loadedTexture.Texture;
}