using System;
using System.Linq;
using LevelImposter.AssetLoader;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Core;
using LevelImposter.Core.Components;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Configures the SpriteRenderer on the GameObject
/// </summary>
/// <see cref="MapTarget">Which map target to load assets to/from</see>
public class SpriteBuilder(MapTarget mapTarget = MapTarget.Game) : IElemBuilder
{
    public delegate void SpriteLoadEvent(LIElement element, Sprite loadedSprite);

    public static SpriteLoadEvent? OnSpriteLoad;

    private bool PixelArtMode => mapTarget.GetLoadedMap()?.properties.pixelArtMode ?? false;
    private MapAssetDB? AssetDB => mapTarget.GetLoadedMap()?.MapAssetDB;

    public int Priority =>
        IElemBuilder.HIGH_PRIORITY; // <-- Ensure `SpriteRenderer` is added before other builders that may need it

    public void OnPreBuild()
    {
        OnSpriteLoad = null; // <-- Clears any previous subscriptions
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        // Load Animations
        if (elem.properties.animations != null)
        {
            // Add SpriteRenderer immediately
            obj.GetOrAddComponent<SpriteRenderer>();

            // Add SpriteAnimator
            var spriteAnimator = obj.AddComponent<SpriteAnimator>();
            spriteAnimator.Init(elem, elem.properties.animations, mapTarget);
        }

        // Load Sprite
        if (elem.properties.spriteID == null)
            return;

        // Add SpriteRenderer immediately
        var spriteRenderer = obj.GetOrAddComponent<SpriteRenderer>();

        // Load Sprite
        LoadSprite(elem, sprite =>
        {
            // Set sprite if no animation is playing
            var animator = obj.GetComponent<LIAnimatorBase>();
            var animating = animator != null && animator.IsAnimating;
            if (!animating)
                spriteRenderer.sprite = sprite;

            // Check if loaded sprite is a GIF
            if (sprite.TextureResult is GIFLoader.GifTextureResult gifTexture)
            {
                var gifAnimator = obj.AddComponent<GIFAnimator>();
                gifAnimator.Init(elem, gifTexture.GIFFile);
            }

            // Invoke Callback
            try
            {
                OnSpriteLoad?.Invoke(elem, sprite);
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
    public void LoadSprite(LIElement elem, Action<SpriteResult> onLoad)
    {
        // Get LoadableSprite
        var loadableSprite = GetLoadableFromID(elem.properties.spriteID);
        if (loadableSprite == null)
            return;

        // Add to queue
        SpriteLoader.Instance.AddToQueue((SpriteInfo)loadableSprite, onLoad);
    }

    /// <summary>
    ///     Finds a sprite atlas by its ID
    /// </summary>
    /// <param name="id">ID of the sprite atlas</param>
    /// <returns>The sprite atlas or null if not found</returns>
    private static LISpriteAtlas? FindSpriteAtlasOfID(Guid? id)
    {
        var allSpriteAtlases = GameConfiguration.CurrentMap?.spriteAtlases;
        return allSpriteAtlases?.FirstOrDefault(atlas => atlas.id == id);
    }

    /// <summary>
    ///     Gets a LoadableSprite from a sprite ID.
    ///     Tries to find a sprite atlas first, then falls back to normal sprite.
    /// </summary>
    /// <param name="spriteID">ID of the sprite</param>
    /// <returns>The LoadableSprite or null if the ID is null</returns>
    public SpriteInfo? GetLoadableFromID(Guid? spriteID)
    {
        // Check for null ID
        if (spriteID == null)
            return null;

        // Get Sprite Atlas
        var spriteAtlas = FindSpriteAtlasOfID(spriteID);
        if (spriteAtlas != null)
            return GetLoadableFromSpriteAtlas(spriteAtlas);

        // Fallback to normal sprite
        var asset = AssetDB?.Get(spriteID);
        if (asset == null)
        {
            LILogger.Warn($"Could not find asset for sprite {spriteID}");
            return null;
        }

        // Create LoadableTexture
        var loadableTexture = new TextureInfo(spriteID.ToString() ?? "", asset);
        loadableTexture.Options.GCBehavior = mapTarget.GetGCBehavior();
        loadableTexture.Options.PixelArt = PixelArtMode;

        // Create LoadableSprite
        return SpriteInfo.FromLoadableTexture(loadableTexture);
    }

    /// <summary>
    ///     Creates a LoadableSprite from a sprite atlas
    /// </summary>
    /// <param name="spriteAtlas">Sprite atlas to reference</param>
    /// <returns>The LoadableSprite</returns>
    /// <exception cref="Exception">Thrown if the asset is not found in the AssetDB</exception>
    private SpriteInfo? GetLoadableFromSpriteAtlas(LISpriteAtlas spriteAtlas)
    {
        // Get Asset from AssetDB
        var baseAssetID = spriteAtlas.assetID;
        var baseAsset = AssetDB?.Get(baseAssetID);
        if (baseAsset == null)
        {
            LILogger.Warn($"Could not find asset for sprite {baseAssetID}");
            return null;
        }

        // Create LoadableTexture
        var loadableTexture = new TextureInfo(baseAssetID.ToString(), baseAsset);
        loadableTexture.Options.GCBehavior = mapTarget.GetGCBehavior();
        loadableTexture.Options.PixelArt = PixelArtMode;

        // Create LoadableSprite
        var loadableSprite = new SpriteInfo(spriteAtlas.id.ToString(), loadableTexture);
        loadableSprite.Options.GCBehavior = mapTarget.GetGCBehavior();
        loadableSprite.Options.Frame = new Rect(
            spriteAtlas.x,
            spriteAtlas.y,
            spriteAtlas.w,
            spriteAtlas.h
        );

        return loadableSprite;
    }

    /// <summary>
    ///     Loads all sprites into cache for the current map.
    /// </summary>
    public static void PreloadAllMapSprites()
    {
        // Get current map
        var map = GameConfiguration.CurrentMap;
        if (map == null)
            return;

        // Log
        LILogger.Info("Preloading all sprites for " + map.name);

        // Start Async Loading
        GCHandler.SetDefaultBehavior(GCBehavior.DisposeOnMapUnload);
        var spriteBuilder = new SpriteBuilder();
        foreach (var elem in map.elements)
            if (elem.properties.spriteID != null)
                spriteBuilder.LoadSprite(elem, _ => { });
    }
}