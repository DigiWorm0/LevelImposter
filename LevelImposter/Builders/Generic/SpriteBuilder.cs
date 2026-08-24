using System;
using System.Linq;
using LevelImposter.AssetLoader;
using LevelImposter.AssetLoader.Loadables;
using LevelImposter.AssetLoader.Loaders;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using LevelImposter.Shop.Components;
using UnityEngine;

namespace LevelImposter.Builders.Generic;

/// <summary>
///     Configures the SpriteRenderer on the GameObject
/// </summary>
/// <see cref="MapTarget">Which map target to load assets to/from</see>
internal static class SpriteBuilder
{
    public delegate void SpriteLoadEvent(LIElement element, Sprite loadedSprite);

    public static SpriteLoadEvent? OnSpriteLoad;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        OnSpriteLoad = null; // <-- Clears any previous subscriptions
    }

    [ElementBuilder(Priority = Priority.FIRST)]
    public static void Build(LIMap map, LIElement element, GameObject gameObject)
    {
        // Load Animations
        if (element.properties.animations != null)
        {
            // Add SpriteRenderer immediately
            gameObject.GetOrAddComponent<SpriteRenderer>();

            // Add SpriteAnimator
            var spriteAnimator = gameObject.AddComponent<SpriteAnimator>();
            spriteAnimator.Init(element, element.properties.animations, map);
        }

        // Load Sprite
        if (element.properties.spriteID == null)
            return;

        // Add SpriteRenderer immediately
        var spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();

        // Load Sprite
        LoadSprite(map, element, sprite =>
        {
            // Set sprite if no animation is playing
            var animator = gameObject.GetComponent<LIAnimatorBase>();
            var animating = animator != null && animator.IsAnimating;
            if (!animating)
                spriteRenderer.sprite = sprite;

            // Check if loaded sprite is a GIF
            if (sprite.TextureResult is GIFLoader.GifTextureResult gifTexture)
            {
                var gifAnimator = gameObject.AddComponent<GIFAnimator>();
                gifAnimator.Init(element, gifTexture.GIFFile);
            }

            // Invoke Callback
            try
            {
                OnSpriteLoad?.Invoke(element, sprite);
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
    public static void LoadSprite(LIMap map, LIElement elem, Action<SpriteResult> onLoad)
    {
        // Get LoadableSprite
        var loadableSprite = GetLoadableFromID(elem.properties.spriteID, map);
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
    public static SpriteInfo? GetLoadableFromID(Guid? spriteID, LIMap map)
    {
        // Check for null ID
        if (spriteID == null)
            return null;

        // Get Sprite Atlas
        var spriteAtlas = FindSpriteAtlasOfID(spriteID);
        if (spriteAtlas != null)
            return GetLoadableFromSpriteAtlas(spriteAtlas, map);

        // Fallback to normal sprite
        var asset = map.MapAssetDB?.Get(spriteID);
        if (asset == null)
        {
            LILogger.Warn($"Could not find asset for sprite {spriteID}");
            return null;
        }

        // Create LoadableTexture
        var loadableTexture = new TextureInfo(
            $"{spriteID}_{map.MapTarget}", // <-- Ensures different IDs for different map targets
            asset);
        loadableTexture.Options.GCBehavior = map.MapTarget.GetGCBehavior();
        loadableTexture.Options.PixelArt = false; // TODO: FIX ME

        // Create LoadableSprite
        return SpriteInfo.FromLoadableTexture(loadableTexture);
    }

    /// <summary>
    ///     Creates a LoadableSprite from a sprite atlas
    /// </summary>
    /// <param name="spriteAtlas">Sprite atlas to reference</param>
    /// <returns>The LoadableSprite</returns>
    /// <exception cref="Exception">Thrown if the asset is not found in the AssetDB</exception>
    private static SpriteInfo? GetLoadableFromSpriteAtlas(LISpriteAtlas spriteAtlas, LIMap map)
    {
        // Get Asset from AssetDB
        var baseAssetID = spriteAtlas.assetID;
        var baseAsset = map.MapAssetDB?.Get(baseAssetID);
        if (baseAsset == null)
        {
            LILogger.Warn($"Could not find asset for sprite {baseAssetID}");
            return null;
        }

        // Create LoadableTexture
        var loadableTexture = new TextureInfo($"{baseAssetID}_{map.MapTarget}", baseAsset);
        loadableTexture.Options.GCBehavior = map.MapTarget.GetGCBehavior();
        loadableTexture.Options.PixelArt = map.properties.pixelArtMode ?? false;

        // Create LoadableSprite
        var loadableSprite = new SpriteInfo($"{spriteAtlas.id}_{map.MapTarget}", loadableTexture);
        loadableSprite.Options.GCBehavior = map.MapTarget.GetGCBehavior();
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
        // Only run in the lobby
        if (!GameState.IsInLobby)
            return;

        // Get current map
        var map = GameConfiguration.CurrentMap;
        if (map == null)
            return;

        // Log
        LILogger.Info("Preloading all sprites for " + map.name);

        // Start Async Loading
        GCHandler.SetDefaultBehavior(GCBehavior.DisposeOnMapUnload);
        foreach (var elem in map.elements)
            if (elem.properties.spriteID != null)
                LoadSprite(map, elem, _ => { });

        // Show Loading Bar UI
        LoadingBar.Run();
    }

    /// <summary>
    ///     Attaches global event listeners
    /// </summary>
    public static void Init()
    {
        GameConfiguration.OnMapChange += PreloadAllMapSprites;
    }
}