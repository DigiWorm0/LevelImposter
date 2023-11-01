using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Multi-threaded sprite loader
    /// </summary>
    public class SpriteLoader : MonoBehaviour
    {
        public SpriteLoader(IntPtr intPtr) : base(intPtr)
        {
        }

        public static SpriteLoader? Instance { get; private set; }

        [HideFromIl2Cpp]
        public event SpriteEventHandle? OnLoad;
        public delegate void SpriteEventHandle(LIElement elem);

        private Stack<SpriteData>? _spriteCache = new();
        private int _renderCount = 0;

        public int RenderCount => _renderCount;

        /// <summary>
        /// Searches for a Sprite in cache by GUID
        /// </summary>
        /// <param name="spriteID">GUID of the sprite or associated object</param>
        /// <returns>Sprite from cache or NULL if none found</returns>
        [HideFromIl2Cpp]
        private SpriteData? GetSpriteFromCache(string? spriteID)
        {
            if (_spriteCache == null || string.IsNullOrEmpty(spriteID))
                return null;
            foreach (SpriteData spriteData in _spriteCache)
            {
                if (spriteData.ID == spriteID)
                {
                    LILogger.Info($" + Found sprite in cache [{spriteID}]");
                    return spriteData;
                }
            }
            return null;
        }

        /// <summary>
        /// Cleans the sprite cache
        /// </summary>
        public void Clean()
        {
            OnLoad = null;
            _spriteCache?.Clear();
        }

        /// <summary>
        /// Gets whether or not a sprite is in the sprite cache
        /// </summary>
        /// <param name="spriteID">GUID of the sprite or associated object</param>
        /// <returns>True iff the sprite is in the sprite cache</returns>
        public bool IsSpriteInCache(string? spriteID)
        {
            if (_spriteCache == null || string.IsNullOrEmpty(spriteID))
                return false;
            return _spriteCache.Any((spriteData) => spriteData.ID == spriteID);
        }

        /// <summary>
        /// Adds sprite data to the sprite cache
        /// </summary>
        /// <param name="data">Sprite data to add</param>
        [HideFromIl2Cpp]
        private void AddSpriteToCache(SpriteData? data)
        {
            if (data == null || string.IsNullOrEmpty(data.ID))
                return;
            _spriteCache?.Push(data);
        }

        /// <summary>
        /// Loads a custom sprite from an
        /// LIElement and applies loads it
        /// onto a GameObject asynchronously
        /// </summary>
        /// <param name="element">LIElement to grab sprite from</param>
        /// <param name="obj">GameObject to apply sprite data to</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(LIElement element, GameObject obj)
        {
            LILogger.Info($"Loading sprite for {element}");
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Get Sprite Data
            var spriteDB = LIShipStatus.Instance?.CurrentMap?.mapAssetDB;
            Guid? spriteID = element.properties.spriteID;
            var spriteStream = spriteDB?.Get(spriteID)?.OpenStream();
            if (spriteStream == null)
            {
                LILogger.Warn($"Could not find sprite for {element}");
                return;
            }

            LoadSpriteAsync(spriteStream, (nullableSpriteData) =>
            {
                // Abort on Exit
                if (obj == null)
                    return;

                // Handle Error
                if (nullableSpriteData == null)
                {
                    LILogger.Warn($"Error loading sprite for {element}");
                    return;
                }

                // Load Components
                var spriteData = nullableSpriteData;
                if (spriteData.IsGIF) // Animated GIF
                {
                    GIFAnimator gifAnimator = obj.AddComponent<GIFAnimator>();
                    gifAnimator.Init(element, spriteData.GIFData);
                    stopwatch.Stop();
                    LILogger.Info($"Done loading {spriteData.GIFData?.Width}x{spriteData.GIFData?.Height} sprite for {element} ({RenderCount} Left) [{stopwatch.ElapsedMilliseconds}ms]");
                }
                else // Still Image
                {
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = spriteData.Sprite;
                    Rect spriteDim = spriteData.Sprite?.rect ?? new();
                    stopwatch.Stop();
                    LILogger.Info($"Done loading {spriteDim.width}x{spriteDim.height} sprite for {element} ({RenderCount} Left) [{stopwatch.ElapsedMilliseconds}ms]");
                }

                if (OnLoad != null)
                    OnLoad.Invoke(element);
            }, spriteID.ToString(), null);
        }

        /// <summary>
        /// Loads the custom sprite in a seperate thread
        /// </summary>
        /// <param name="b64Image">Base64 image data to read</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(string b64Image, Action<SpriteData?> onLoad, string? spriteID, Vector2? pivot)
        {
            var imgData = string.IsNullOrEmpty(b64Image) ? new(0) : MapUtils.ParseBase64(b64Image);
            var imgStream = new MemoryStream(imgData);
            LoadSpriteAsync(imgStream, (spriteList) =>
            {
                onLoad(spriteList);
                spriteList = null;
            }, spriteID, pivot);
        }


        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(Stream? imgStream, Action<SpriteData?> onLoad, string? spriteID, Vector2? pivot)
        {
            StartCoroutine(CoLoadSpriteAsync(imgStream, onLoad, spriteID, pivot).WrapToIl2Cpp());
        }

        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        private IEnumerator CoLoadSpriteAsync(Stream? imgStream, Action<SpriteData?>? onLoad, string? spriteID, Vector2? pivot)
        {
            {
                _renderCount++;
                yield return null;
                yield return null;
                while (!LagLimiter.ShouldContinue(1))
                    yield return null;

                // Search Cache
                SpriteData? spriteData = GetSpriteFromCache(spriteID);
                if (spriteData != null)
                {
                    // Using sprite data from cache
                }
                else if (imgStream == null)
                {
                    LILogger.Warn($"Could not load sprite {spriteID} from null stream");
                    spriteData = null;
                }
                else if (GIFFile.IsGIF(imgStream))
                {
                    // Generate GIF Data
                    var gifFile = new GIFFile(spriteID ?? "LISprite");
                    gifFile.SetPivot(pivot);
                    gifFile.Load(imgStream);

                    // Create Texture
                    spriteData = new()
                    {
                        ID = spriteID ?? "",
                        Sprite = gifFile.GetFrameSprite(0),
                        GIFData = gifFile
                    };
                    AddSpriteToCache(spriteData);
                    GCHandler.Register(spriteData);
                }
                else
                {
                    // Get All Data
                    byte[] imageDataBuffer = new byte[imgStream.Length];
                    imgStream.Read(imageDataBuffer, 0, imageDataBuffer.Length);

                    // Create Texture
                    spriteData = new()
                    {
                        ID = spriteID ?? "",
                        Sprite = RawImageToSprite(imageDataBuffer, pivot)
                    };
                    spriteData.Sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    AddSpriteToCache(spriteData);
                    GCHandler.Register(spriteData);
                }

                // Output
                _renderCount--;
                if (onLoad != null)
                    onLoad.Invoke(spriteData);

                onLoad = null;
                imgStream?.Dispose();
                imgStream = null;
            }
        }

        /// <summary>
        /// Loads sprite from texture metadata.
        /// MUST be ran from the main Unity thread.
        /// </summary>
        /// <param name="texData">Texture Metadata to load</param>
        /// <returns>Generated sprite data</returns>
        [HideFromIl2Cpp]
        private Sprite RawImageToSprite(Il2CppStructArray<byte> imgData, Vector2? pivot)
        {
            // Generate Texture
            bool pixelArtMode = LIShipStatus.Instance?.CurrentMap?.properties.pixelArtMode ?? false;
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
                requestedMipmapLevel = 0
            };
            ImageConversion.LoadImage(texture, imgData);

            // Generate Sprite
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                pivot ?? new Vector2(0.5f, 0.5f),
                100.0f,
                0,
                SpriteMeshType.FullRect
            );

            return sprite;
        }

        /// <summary>
        /// Loads a sprite from byte data synchronously.
        /// </summary>
        /// <param name="imgData">Raw image file data</param>
        /// <returns>Unity Sprite object</returns>
        [HideFromIl2Cpp]
        public Sprite LoadSprite(Il2CppStructArray<byte> imgData, string? spriteID)
        {
            SpriteData? spriteData = GetSpriteFromCache(spriteID);
            Sprite? sprite = spriteData?.Sprite;
            if (sprite == null)
                sprite = RawImageToSprite(imgData, null);
            return sprite;
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Metadata to store and send animated texture data
        /// </summary>
        public class SpriteData : IDisposable
        {
            public SpriteData() { }
            public string ID = "";
            public Sprite? Sprite { get; set; }

            public bool IsGIF => GIFData != null;
            public GIFFile? GIFData { get; set; }

            public void Dispose()
            {
                if (GIFData != null)
                    GIFData.Dispose();
                if (Sprite != null)
                {
                    Destroy(Sprite.texture);
                    Destroy(Sprite);
                }
                Sprite = null;
                GIFData = null;
            }
        }
    }
}
