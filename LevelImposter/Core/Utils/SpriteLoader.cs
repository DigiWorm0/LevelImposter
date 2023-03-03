using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using BepInEx.Unity.IL2CPP.Utils.Collections;

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

        public const float MIN_FRAMERATE = 10.0f;
        public readonly List<string> CONVERT_TYPES = new()
        {
            "data:image/webp",
            "data:image/gif"
        };

        public static SpriteLoader? Instance;

        [HideFromIl2Cpp]
        public event SpriteEventHandle? OnLoad;
        public delegate void SpriteEventHandle(LIElement elem);

        private Stack<SpriteData>? _spriteList = new();
        private Stopwatch? _renderTimer = new();
        private int _renderCount = 0;
        private bool _shouldRender
        {
            get { return _renderTimer?.ElapsedMilliseconds <= (1000.0f / MIN_FRAMERATE); }
        }

        public int RenderCount => _renderCount;

        /// <summary>
        /// Marks all sprites and textures for garbage collection
        /// </summary>
        public void ClearAll()
        {
            LILogger.Info($"Destroying {_spriteList?.Count} sprites");
            while (_spriteList?.Count > 0)
            {
                SpriteData spriteData = _spriteList.Pop();
                foreach (Sprite sprite in spriteData.SpriteArr)
                {
                    try
                    {
                        Destroy(sprite?.texture);
                        Destroy(sprite);
                    }
                    catch (Exception e)
                    {
                        LILogger.Warn(e);
                    }
                }
            }
            OnLoad = null;
            _renderCount = 0;
        }

        /// <summary>
        /// Adds a sprite data to the stack
        /// </summary>
        /// <param name="sprite">Sprite to add to managed stack</param>
        public void AddSpriteData(SpriteData spriteData)
        {
            _spriteList?.Push(spriteData);
            foreach (Sprite sprite in spriteData.SpriteArr)
                sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        /// <summary>
        /// Searches for a Sprite in cache by GUID
        /// </summary>
        /// <param name="spriteID">GUID of the sprite or associated object</param>
        /// <returns>Sprite from cache or NULL if none found</returns>
        private SpriteData? GetSpriteFromCache(string? spriteID)
        {
            if (_spriteList == null || string.IsNullOrEmpty(spriteID))
                return null;
            foreach (SpriteData spriteData in _spriteList)
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
                var spriteData = (SpriteData)nullableSpriteData;

                if (spriteData.IsAnimated) // Animated GIF
                {
                    GIFAnimator gifAnimator = obj.AddComponent<GIFAnimator>();
                    gifAnimator.Init(element, spriteData.SpriteArr, spriteData.FrameDelayArr);
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
            var imgData = MapUtils.ParseBase64(b64Image);
            bool shouldConvert = CONVERT_TYPES.Find((prefix) => b64Image.StartsWith(prefix)) != null;
            LoadSpriteAsync(imgData, shouldConvert, (spriteList) =>
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
        public void LoadSpriteAsync(byte[] imgData, bool useImageSharp, Action<SpriteData?> onLoad, string? spriteID)
        {
            StartCoroutine(CoLoadSpriteAsync(imgData, useImageSharp, onLoad, spriteID).WrapToIl2Cpp());
        }

        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        private IEnumerator CoLoadSpriteAsync(byte[] imgData, bool useImageSharp, Action<SpriteData?>? onLoad, string? spriteID)
        {
            {
                _renderCount++;
                yield return null;
                while (!_shouldRender)
                    yield return null;

                // Search Cache
                SpriteData? spriteData = GetSpriteFromCache(spriteID);
                if (spriteData != null)
                {
                    // Using sprite data from cache
                }
                else if (useImageSharp && ImageSharpWrapper.IsInstalled)
                {
                    var texMetadataList = ImageSharpWrapper.LoadImage(imgData);
                    if (texMetadataList != null)
                    {
                        var spriteArr = new Sprite[texMetadataList.Length];
                        var frameDelayArr = new float[texMetadataList.Length];
                        for (int i = 0; i < texMetadataList.Length; i++)
                        {
                            while (!_shouldRender)
                                yield return null;
                            ImageSharpWrapper.TextureMetadata texData = texMetadataList[i];
                            Sprite sprite = RawImageToSprite(texData.FrameData);
                            spriteArr[i] = sprite;
                            frameDelayArr[i] = texData.FrameDelay;
                        }
                        spriteData = new()
                        {
                            ID = spriteID ?? "",
                            SpriteArr = spriteArr,
                            FrameDelayArr = frameDelayArr
                        };
                        AddSpriteData((SpriteData)spriteData);
                    }
                }
                else
                {
                    spriteData = new()
                    {
                        ID = spriteID ?? "",
                        SpriteArr = new Sprite[]
                        {
                            RawImageToSprite(imgData)
                        },
                        FrameDelayArr = new float[1]
                    };
                    AddSpriteData((SpriteData)spriteData);
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
        private Sprite RawImageToSprite(byte[] imgData)
        {
            // Generate Texture
            bool pixelArtMode = LIShipStatus.Instance?.CurrentMap?.properties.pixelArtMode == true;
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
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
        public Sprite LoadSprite(byte[] imgData, string? spriteID)
        {
            SpriteData? spriteData = GetSpriteFromCache(spriteID);
            Sprite? sprite = spriteData?.Sprite;
            if (sprite == null)
            {
                sprite = RawImageToSprite(imgData);
                spriteData = new()
                {
                    ID = spriteID ?? "",
                    SpriteArr = new Sprite[1] { sprite },
                    FrameDelayArr = new float[1],
                };
                AddSpriteData((SpriteData)spriteData);
            }
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
        public struct SpriteData
        {
            public SpriteData() { }
            public string ID = "";
            public Sprite[] SpriteArr = Array.Empty<Sprite>();
            public float[] FrameDelayArr = Array.Empty<float>();
            public Sprite? Sprite
            {
                get
                {
                    return SpriteArr.Length > 0 ? SpriteArr[0] : null;
                }
            }
            public bool IsAnimated
            {
                get
                {
                    return SpriteArr.Length > 1;
                }
            }
        }
    }
}
