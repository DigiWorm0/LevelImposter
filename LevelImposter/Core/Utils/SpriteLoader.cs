using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using LevelImposter.Shop;
using LevelImposter.DB;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;
using System.Threading;

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

        private Stopwatch _renderTimer = new();
        private int _renderCount = 0;
        private Stack<Texture2D> _mapTextures = new();

        public int RenderCount => _renderCount;

        [HideFromIl2Cpp]
        public event SpriteEventHandle OnLoad;
        public delegate void SpriteEventHandle(LIElement elem);

        public static SpriteLoader Instance;

        public void Awake()
        {
            Instance = this;
        }
        public void Update()
        {
            if (_renderCount > 0)
                _renderTimer.Restart();
        }
        public void OnDestroy()
        {
            LILogger.Info("Destroying " + _mapTextures.Count + " map textures");
            while (_mapTextures.Count > 0)
            {
                Texture2D texture = _mapTextures.Pop();
                Destroy(texture);
                LILogger.Info(texture);
            }
            GC.Collect();
            Resources.UnloadUnusedAssets();
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
            LoadSprite(b64, (spriteArr) =>
            {
                // Handle Error
                if (spriteArr == null)
                {
                    LILogger.Warn($"Error loading sprite for {element}");
                    return;
                }

                if (spriteArr.isAnimated) // Animated GIF
                {
                    GIFAnimator gifAnimator = obj.AddComponent<GIFAnimator>();
                    gifAnimator.Init(element, spriteArr.spriteArr, spriteArr.frameTimeArr);
                    LILogger.Info($"Done loading animated sprite for {element}");
                }
                else // Still Image
                {
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = spriteArr.sprite;
                    LILogger.Info($"Done loading sprite for {element}");
                }

                if (OnLoad != null)
                    OnLoad.Invoke(element);
            });
        }

        /// <summary>
        /// Loads the custom sprite in a seperate thread
        /// </summary>
        /// <param name="b64Image">Base64 image data to read</param>
        /// <param name="onLoad">Callback on success w/ an array of Sprites and Frame Delays measured in seconds</param>
        [HideFromIl2Cpp]
        public void LoadSprite(string b64Image, Action<SpriteList> onLoad)
        {
            byte[] imgBytes = MapUtils.ParseBase64(b64Image);
            LoadSprite(imgBytes, onLoad);
        }

        /// <summary>
        /// Loads the custom sprite in a seperate thread
        /// </summary>
        /// <param name="imgBytes">Array of bytes representing image data</param>
        /// <param name="onLoad">Callback on success w/ an array of Sprites and Frame Delays measured in seconds</param>
        [HideFromIl2Cpp]
        public void LoadSprite(byte[] imgBytes, Action<SpriteList> onLoad)
        {
            StartCoroutine(CoLoadElement(imgBytes, (spriteList) =>
            {
                onLoad(spriteList);
                spriteList = null;
            }).WrapToIl2Cpp());
        }

        [HideFromIl2Cpp]
        private IEnumerator CoLoadElement(byte[] imgBytes, Action<SpriteList> onLoad)
        {
            _renderCount++;

            // Run Task
            FreeImageWrapper.TextureList textureList = null;
            Task<bool> task = Task.Run(() => {
                return FreeImageWrapper.LoadImage(imgBytes, out textureList);
            });
            while (!task.IsCompleted)
                yield return null;

            // Get Output
            bool isSuccess = task.Result;
            if (!isSuccess)
            {
                onLoad(null);
                yield break;
            }

            // Sprite List
            SpriteList spriteList = new SpriteList();
            spriteList.spriteArr = new Sprite[textureList.texDataArr.Length];
            spriteList.frameTimeArr = textureList.frameTimeArr;
            for (int i = 0; i < textureList.texDataArr.Length; i++)
            {
                while (_renderTimer.ElapsedMilliseconds > (1000 / 15)) // Stay above ~15fps
                    yield return null;
                FreeImageWrapper.TextureMetadata texData = textureList.texDataArr[i];
                Sprite sprite = LoadImage(texData);
                spriteList.spriteArr[i] = sprite;
                texData.texBytes = Array.Empty<byte>();
                texData = null;
            }
            textureList = null;

            // Output
            onLoad.Invoke(spriteList);
            _renderCount--;
        }

        /// <summary>
        /// Loads sprite from texture metadata.
        /// MUST be ran from the main Unity thread.
        /// </summary>
        /// <param name="texData">Texture Metadata to load</param>
        /// <returns>Generated sprite data</returns>
        [HideFromIl2Cpp]
        private Sprite LoadImage(FreeImageWrapper.TextureMetadata texData)
        {
            // Generate Texture
            bool pixelArtMode = LIShipStatus.Instance.CurrentMap.properties.pixelArtMode == true;
            Texture2D texture = new(
                texData.width,
                texData.height,
                TextureFormat.BGRA32,
                1,
                false
            )
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
            };
            texture.LoadRawTextureData(texData.texBytes);
            texture.Apply(false, true);
            _mapTextures.Push(texture);

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
        /// Metadata to store and send animated texture data
        /// </summary>
        public class SpriteList
        {
            public Sprite[] spriteArr = Array.Empty<Sprite>();
            public float[] frameTimeArr = Array.Empty<float>();
            public Sprite sprite
            {
                get
                {
                    return spriteArr.Length > 0 ? spriteArr[0] : null;
                }
            }
            public bool isAnimated
            {
                get
                {
                    return spriteArr.Length > 1;
                }
            }
        }
    }
}
