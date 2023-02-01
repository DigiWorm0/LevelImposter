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

        public const float MIN_FRAMERATE = 15.0f;
        public readonly List<string> CONVERT_TYPES = new()
        {
            "data:image/webp",
            "data:image/gif"
        };

        public static SpriteLoader? Instance;

        [HideFromIl2Cpp]
        public event SpriteEventHandle? OnLoad;
        public delegate void SpriteEventHandle(LIElement elem);

        private Stack<Texture2D>? _mapTextures = new();
        private Stack<Sprite>? _mapSprites = new();
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
            LILogger.Info($"Destroying {_mapTextures?.Count} textures ({_mapSprites?.Count} sprites)");
            while (_mapTextures?.Count > 0)
                Destroy(_mapTextures.Pop());
            while (_mapSprites?.Count > 0)
                Destroy(_mapSprites.Pop());
            OnLoad = null;
            _renderCount = 0;
        }

        /// <summary>
        /// Adds a sprite to garbage collection list
        /// </summary>
        /// <param name="sprite">Sprite to garbage collect on exit</param>
        public void AddSprite(Sprite sprite)
        {
            _mapSprites?.Push(sprite);
        }

        /// <summary>
        /// Adds a texture to garbage collection list
        /// </summary>
        /// <param name="texture">Texture2D to garbage collect on exit</param>
        public void AddTexture(Texture2D texture)
        {
            _mapTextures?.Push(texture);
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
            });
        }

        /// <summary>
        /// Loads the custom sprite in a seperate thread
        /// </summary>
        /// <param name="b64Image">Base64 image data to read</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(string b64Image, Action<SpriteData?> onLoad)
        {
            var imgData = MapUtils.ParseBase64(b64Image);
            bool shouldConvert = CONVERT_TYPES.Find((prefix) => b64Image.StartsWith(prefix)) != null;
            LoadSpriteAsync(imgData, shouldConvert, (spriteList) =>
            {
                onLoad(spriteList);
                spriteList = null;
            });
        }


        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSpriteAsync(byte[] imgData, bool useImageSharp, Action<SpriteData?> onLoad)
        {
            StartCoroutine(CoLoadSpriteAsync(imgData, useImageSharp, onLoad).WrapToIl2Cpp());
        }

        /// <summary>
        /// Loads a custom sprite asynchronously from raw image file data
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        private IEnumerator CoLoadSpriteAsync(byte[] imgData, bool useImageSharp, Action<SpriteData?>? onLoad)
        {
            {
                _renderCount++;
                yield return null;
                while (!_shouldRender)
                    yield return null;

                // Get Output
                SpriteData? spriteData = null;
                if (useImageSharp)
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
                            Sprite sprite = RawImageToSprite(texData.FrameData, true);
                            spriteArr[i] = sprite;
                            frameDelayArr[i] = texData.FrameDelay;
                        }
                        spriteData = new()
                        {
                            SpriteArr = spriteArr,
                            FrameDelayArr = frameDelayArr
                        };
                    }
                }
                else
                {
                    spriteData = new()
                    {
                        SpriteArr = new Sprite[]
                        {
                            RawImageToSprite(imgData, true)
                        },
                        FrameDelayArr = new float[1]
                    };
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
        private Sprite RawImageToSprite(byte[] imgData, bool addToStack)
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
            if (addToStack)
                _mapTextures?.Push(texture);

            // Generate Sprite
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f,
                0,
                SpriteMeshType.FullRect
            );
            if (addToStack)
                _mapSprites?.Push(sprite);

            return sprite;
        }

        /// <summary>
        /// Loads a sprite from byte data synchronous.
        /// Does not get garbage collected on unload.
        /// </summary>
        /// <param name="imgData">Raw image file data</param>
        /// <returns>Unity Sprite object</returns>
        [HideFromIl2Cpp]
        public Sprite LoadSprite(byte[] imgData)
        {
            return RawImageToSprite(imgData, false);
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
