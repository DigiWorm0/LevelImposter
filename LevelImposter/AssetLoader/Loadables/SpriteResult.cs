using LevelImposter.AssetLoader.Queue;
using UnityEngine;

namespace LevelImposter.AssetLoader.Loadables;

public class SpriteResult(Sprite sprite, TextureResult textureResult) : ICachable
{
    public Sprite Sprite => sprite;
    public TextureResult TextureResult => textureResult;
    public bool IsExpired => sprite == null || textureResult.IsExpired;

    public static implicit operator Sprite(SpriteResult spriteResult)
    {
        return spriteResult.Sprite;
    }
}