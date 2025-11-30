using System;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using LevelImposter.Shop;
using UnityEngine;
using System.Linq;

namespace LevelImposter.Builders;

/// <summary>
///     Configures the SpriteRenderer on the GameObject
/// </summary>
public class SpriteBuilder : IElemBuilder
{
    public delegate void SpriteLoadEvent(LIElement element, LoadedSprite loadedSprite);

    public static SpriteLoadEvent? OnSpriteLoad;

    public SpriteBuilder()
    {
        OnSpriteLoad = null;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.properties.spriteID == null)
            return;

        // Add SpriteRenderer
        var spriteRenderer = obj.AddComponent<SpriteRenderer>();

        // Load Sprite
        LoadSprite(elem, loadedSprite =>
        {
            // Set Sprite
            spriteRenderer.sprite = loadedSprite.Sprite;
            
            // Handle Sprite Anim
            if (elem.properties.animation != null)
            {
                // Add SpriteAnimator
                var spriteAnimator = obj.AddComponent<SpriteAnimator>();
                spriteAnimator.Init(elem, elem.properties.animation);
            }

            // Handle GIF
            if (loadedSprite is GIFLoader.LoadedGIF gifData)
            {
                var gifAnimator = obj.AddComponent<GIFAnimator>();
                gifAnimator.Init(elem, gifData.GIFFile);
            }

            // Invoke Callback
            try
            {
                OnSpriteLoad?.Invoke(elem, loadedSprite);
            }
            catch (Exception e)
            {
                LILogger.Error(e);
            }
        });
    }

    /// <summary>
    ///     Loads a sprite from an LIElement. Can also be used to preload sprites.
    /// </summary>
    /// <param name="elem">Element to load</param>
    /// <param name="onLoad">Callback when the sprite is loaded</param>
    /// <exception cref="Exception">Thrown if the sprite asset is not found in the AssetDB</exception>
    public static void LoadSprite(LIElement elem, Action<LoadedSprite> onLoad)
    {
        // Get Asset from AssetDB
        var assetDB = MapLoader.CurrentMap?.mapAssetDB;
        var asset = assetDB?.Get(elem.properties.spriteID);
        if (asset == null)
            throw new Exception($"Asset {elem.properties.spriteID} not found");

        // Create LoadableSprite
        var loadableSprite = new LoadableSprite(elem.properties.spriteID.ToString() ?? "", asset)
        {
            Options =
            {
                PixelArt = MapLoader.CurrentMap?.properties.pixelArtMode ?? false
            }
        };

        // Add to queue
        SpriteLoader.Instance.AddToQueue(loadableSprite, onLoad);
    }
}