using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Shop;
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

        public const float MIN_FRAME_TIME = 1500.0f;

        public static SpriteLoader? Instance;

        [HideFromIl2Cpp]
        public event SpriteEventHandle? OnLoad;
        public delegate void SpriteEventHandle(LIElement elem);

        private Stack<SpriteData>? _spriteCache = new();
        private Stopwatch? _renderTimer = new();
        private int _renderCount = 0;
        private bool _shouldRender
        {
            get { return _renderTimer?.ElapsedMilliseconds <= MIN_FRAME_TIME; }
        }

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

            string b64 = element.properties.spriteData ?? "";
            LoadSpriteAsync(b64, (nullableSpriteData) =>
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

                // Sprite is in cache, we can reduce memory usage
                element.properties.spriteData = "";

                // Load Components
                var spriteData = nullableSpriteData;
                if (spriteData.IsGIF) // Animated GIF
                {
                    GIFAnimator gifAnimator = obj.AddComponent<GIFAnimator>();
                    gifAnimator.Init(element, spriteData.GIFData);
                    LILogger.Info($"Done loading animated sprite for {element} ({RenderCount} Left)");
                }
                else // Still Image
                {
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = spriteData.Sprite;
                    Rect spriteDim = spriteData.Sprite?.rect ?? new();
                    LILogger.Info($"Done loading {spriteDim.width}x{spriteDim.height} sprite for {element} ({RenderCount} Left)");
                }

                if (OnLoad != null)
                    OnLoad.Invoke(element);
            }, element.id.ToString());
        }

        /// <summary>
        /// Loads the custom sprite in a seperate thread
        /// </summary>
        /// <param name="b64Image">Base64 image data to read</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(string b64Image, Action<SpriteData?> onLoad, string? spriteID)
        {
            var imgData = string.IsNullOrEmpty(b64Image) ? new(0) : MapUtils.ParseBase64(b64Image);
            bool isGIF = b64Image.StartsWith("data:image/gif");
            LoadSpriteAsync(imgData, isGIF, (spriteList) =>
            {
                onLoad(spriteList);
                spriteList = null;
            }, spriteID);
        }


        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(Il2CppStructArray<byte> imgData, bool isGIF, Action<SpriteData?> onLoad, string? spriteID)
        {
            StartCoroutine(CoLoadSpriteAsync(imgData, isGIF, onLoad, spriteID).WrapToIl2Cpp());
        }

        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        private IEnumerator CoLoadSpriteAsync(Il2CppStructArray<byte> imgData, bool isGIF, Action<SpriteData?>? onLoad, string? spriteID)
        {
            {
                _renderCount++;
                yield return null;
                yield return null;
                while (!_shouldRender)
                    yield return null;

                // Search Cache
                SpriteData? spriteData = GetSpriteFromCache(spriteID);
                if (spriteData != null)
                {
                    // Using sprite data from cache
                }
                else if (isGIF)
                {
                    using (MemoryStream ms = new(imgData))
                    {
                        var gifFile = new GIFFile();
                        gifFile.Load(ms);

                        spriteData = new()
                        {
                            ID = spriteID ?? "",
                            Sprite = gifFile.GetFrameSprite(0),
                            GIFData = gifFile
                        };
                        AddSpriteToCache(spriteData);
                    }
                }
                else
                {
                    spriteData = new()
                    {
                        ID = spriteID ?? "",
                        Sprite = RawImageToSprite(imgData)
                    };
                    spriteData.Sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    AddSpriteToCache(spriteData);
                }
                GCHandler.Register(spriteData);

                // Output
                _renderCount--;
                if (onLoad != null)
                    onLoad.Invoke(spriteData);

                onLoad = null;
                imgData = null;
            }
        }

        /// <summary>
        /// Loads sprite from texture metadata.
        /// MUST be ran from the main Unity thread.
        /// </summary>
        /// <param name="texData">Texture Metadata to load</param>
        /// <returns>Generated sprite data</returns>
        [HideFromIl2Cpp]
        private Sprite RawImageToSprite(Il2CppStructArray<byte> imgData)
        {
            // Generate Texture
            bool pixelArtMode = LIShipStatus.Instance?.CurrentMap?.properties.pixelArtMode == true;
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
                new Vector2(0.5f, 0.5f),
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
                sprite = RawImageToSprite(imgData);
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
        public void Update()
        {
            if (_renderCount > 0)
                _renderTimer?.Restart();
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
                    Destroy(Sprite.texture);
                Sprite = null;
                GIFData = null;
            }
        }
    }
}
