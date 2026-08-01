using UnityEngine;

namespace LevelImposter.AssetLoader;

public class TextureResult(Texture2D texture) : ICachable
{
    public Texture2D Texture => texture;
    public bool IsExpired => texture == null;

    public static implicit operator Texture2D(TextureResult textureResult)
    {
        return textureResult.Texture;
    }
}