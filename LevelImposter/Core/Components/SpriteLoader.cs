using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using PowerTools;
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
        private Dictionary<string, string>? _duplicateSpriteDB = null;

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
            if (_duplicateSpriteDB?.ContainsKey(spriteID) == true)
            {
                return GetSpriteFromCache(_duplicateSpriteDB[spriteID]);
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
            _duplicateSpriteDB = null;
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
                    stopwatch.Stop();
                    LILogger.Info($"Done loading {spriteData.GIFData.Width}x{spriteData.GIFData.Height} sprite for {element} ({RenderCount} Left) [{stopwatch.ElapsedMilliseconds}ms]");
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
                while (!LagLimiter.ShouldContinue(1))
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
                        var gifFile = new GIFFile(spriteID ?? "LISprite");
                        gifFile.Load(ms);

                        spriteData = new()
                        {
                            ID = spriteID ?? "",
                            Sprite = gifFile.GetFrameSprite(0),
                            GIFData = gifFile
                        };
                        AddSpriteToCache(spriteData);
                        GCHandler.Register(spriteData);
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
                    GCHandler.Register(spriteData);
                }

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

        /// <summary>
        /// Searches the current map for duplicate sprite entries. Optional, improves performance.
        /// TODO: Optimize me! ( O(n^2) )
        /// </summary>
        [HideFromIl2Cpp]
        public void SearchForDuplicateSprites(LIMap map)
        {
            // Already Loaded
            if (_duplicateSpriteDB != null)
                return;

            // Debug Start
            var elems = map.elements;
            Stopwatch sw = Stopwatch.StartNew();
            LILogger.Info($"Searching {elems.Length} elements for duplicate sprites");

            // Iterate through map elements
            _duplicateSpriteDB = new();
            for (int a = 0; a < elems.Length - 1; a++)
            {
                for (int b = a + 1; b < elems.Length; b++)
                {
                    var spriteA = elems[a].properties.spriteData;
                    var spriteB = elems[b].properties.spriteData;

                    if (_duplicateSpriteDB?.ContainsKey(elems[a].id.ToString()) == false
                        && !string.IsNullOrEmpty(spriteA)
                        && spriteA == spriteB)
                    {
                        _duplicateSpriteDB?.Add(elems[b].id.ToString(), elems[a].id.ToString());
                    }
                }
            }

            // Debug End
            sw.Stop();
            LILogger.Info($"Found {_duplicateSpriteDB?.Count} duplicate sprites in {sw.ElapsedMilliseconds}ms");
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
                    Destroy(Sprite.texture);
                Sprite = null;
                GIFData = null;
            }
        }
    }
}
