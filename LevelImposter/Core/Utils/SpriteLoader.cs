using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Events;
using LevelImposter.Shop;
using LevelImposter.DB;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;
using System.Threading;
using Reactor.Utilities;

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
            GC.Collect();
        }

        /// <summary>
        /// Loads a custom sprite from an
        /// LIElement and applies it to an object
        /// </summary>
        /// <param name="element">LIElement to grab sprite from</param>
        /// <param name="obj">GameObject to apply sprite data to</param>
        [HideFromIl2Cpp]
        public void LoadSprite(LIElement element, GameObject obj)
        {
            LILogger.Info($"Loading sprite for {element}");
            
            string b64 = element.properties.spriteData ?? "";
            LoadSprite(b64, (nullableSpriteData) =>
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
                    LILogger.Info($"Done loading sprite for {element} ({RenderCount} Left)");
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
        public void LoadSprite(string b64Image, Action<SpriteData?> onLoad)
        {
            var imgData = MapUtils.ParseBase64(b64Image);
            LoadSprite(imgData, (spriteList) =>
            {
                onLoad(spriteList);
                spriteList = null;
            });
        }


        /// <summary>
        /// Loads the custom sprite in a seperate thread
        /// </summary>
        /// <param name="imgData">Image File Data</param>
        /// <param name="onLoad">Callback on success</param>
        [HideFromIl2Cpp]
        public void LoadSprite(byte[] imgData, Action<SpriteData?> onLoad)
        {
            StartCoroutine(CoLoadSprite(imgData, onLoad).WrapToIl2Cpp());
        }

        [HideFromIl2Cpp]
        private IEnumerator CoLoadSprite(byte[] imgData, Action<SpriteData?>? onLoad)
        {
            _renderCount++;
            yield return null;
            while (!_shouldRender)
                yield return null;

            // Load Texture Metadata
            ImageSharpWrapper.TextureMetadata[]? texMetadataList;
            using (var imgStream = new MemoryStream(imgData))
            {
                texMetadataList = ImageSharpWrapper.LoadImage(imgStream);
            }

            // Get Output
            SpriteData? spriteData = null;
            if (texMetadataList != null)
            {
                var spriteArr = new Sprite[texMetadataList.Length];
                var frameDelayArr = new float[texMetadataList.Length];
                for (int i = 0; i < texMetadataList.Length; i++)
                {
                    while (!_shouldRender)
                        yield return null;
                    ImageSharpWrapper.TextureMetadata texData = texMetadataList[i];
                    Sprite sprite = LoadImage(texData);
                    spriteArr[i] = sprite;
                    frameDelayArr[i] = texData.FrameDelay;
                    texData = new();
                    sprite = null;
                }
                spriteData = new()
                {
                    SpriteArr = spriteArr,
                    FrameDelayArr = frameDelayArr
                };
                spriteArr = null;
                frameDelayArr = null;
            }

            // Output
            _renderCount--;
            if (onLoad != null)
                onLoad.Invoke(spriteData);
            onLoad = null;
            texMetadataList = null;
            spriteData = null;
        }

        /// <summary>
        /// Loads sprite from texture metadata.
        /// MUST be ran from the main Unity thread.
        /// </summary>
        /// <param name="texData">Texture Metadata to load</param>
        /// <returns>Generated sprite data</returns>
        [HideFromIl2Cpp]
        private Sprite LoadImage(ImageSharpWrapper.TextureMetadata texData)
        {
            // Generate Texture
            bool pixelArtMode = LIShipStatus.Instance?.CurrentMap?.properties.pixelArtMode == true;
            Texture2D texture = new(
                texData.Width,
                texData.Height,
                TextureFormat.RGBA32,
                1,
                false
            )
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
            };
            texture.LoadRawTextureData(texData.RawTextureData);
            texture.Apply(false, true);
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
            _mapSprites?.Push(sprite);

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
