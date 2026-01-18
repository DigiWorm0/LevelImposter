using System;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using LevelImposter.Shop;
using UnityEngine;
using System.Linq;
using LevelImposter.DB;

namespace LevelImposter.Builders;

/// <summary>
///     Configures the SpriteRenderer on the GameObject
/// </summary>
public class SpriteBuilder : IElemBuilder
{
    public delegate void SpriteLoadEvent(LIElement element, Sprite loadedSprite);

    public static SpriteLoadEvent? OnSpriteLoad;

    private static bool PixelArtMode => GameConfiguration.CurrentMap?.properties.pixelArtMode ?? false;
    private static MapAssetDB? AssetDB => GameState.IsInLobby ?
        GameConfiguration.CurrentLobbyMap?.mapAssetDB :
        GameConfiguration.CurrentMap?.mapAssetDB;

    public int Priority => IElemBuilder.HIGH_PRIORITY; // <-- Ensure `SpriteRenderer` is added before other builders that may need it
    
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
            spriteAnimator.Init(elem, elem.properties.animations);
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
            if (sprite.Texture is GIFLoader.LoadedGIFTexture gifTexture)
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
    public static void LoadSprite(LIElement elem, Action<LoadedSprite> onLoad)
    {
        // Get LoadableSprite
        var loadableSprite = GetLoadableFromID(elem.properties.spriteID);
        if (loadableSprite == null)
            return;

        // Add to queue
        SpriteLoader.Instance.AddToQueue((LoadableSprite)loadableSprite, onLoad);
    }

    /// <summary>
    /// Finds a sprite atlas by its ID
    /// </summary>
    /// <param name="id">ID of the sprite atlas</param>
    /// <returns>The sprite atlas or null if not found</returns>
    private static LISpriteAtlas? FindSpriteAtlasOfID(Guid? id)
    {
        var allSpriteAtlases = GameConfiguration.CurrentMap?.spriteAtlases;
        return allSpriteAtlases?.FirstOrDefault(atlas => atlas.id == id);
    }

    /// <summary>
    /// Gets a LoadableSprite from a sprite ID.
    /// Tries to find a sprite atlas first, then falls back to normal sprite.
    /// </summary>
    /// <param name="spriteID">ID of the sprite</param>
    /// <returns>The LoadableSprite or null if the ID is null</returns>
    public static LoadableSprite? GetLoadableFromID(Guid? spriteID)
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
        var loadableTexture = new LoadableTexture(spriteID.ToString() ?? "", asset);
        loadableTexture.Options.PixelArt = PixelArtMode;
        
        // Create LoadableSprite
        return LoadableSprite.FromLoadableTexture(loadableTexture);
    }

    /// <summary>
    /// Creates a LoadableSprite from a sprite atlas
    /// </summary>
    /// <param name="spriteAtlas">Sprite atlas to reference</param>
    /// <returns>The LoadableSprite</returns>
    /// <exception cref="Exception">Thrown if the asset is not found in the AssetDB</exception>
    private static LoadableSprite? GetLoadableFromSpriteAtlas(LISpriteAtlas spriteAtlas)
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
        var loadableTexture = new LoadableTexture(baseAssetID.ToString(), baseAsset);
        loadableTexture.Options.PixelArt = PixelArtMode;

        // Create LoadableSprite
        var loadableSprite = new LoadableSprite(spriteAtlas.id.ToString(), loadableTexture);
        loadableSprite.Options.Frame = new Rect(
            spriteAtlas.x,
            spriteAtlas.y,
            spriteAtlas.w,
            spriteAtlas.h
        );

        return loadableSprite;
    }

}