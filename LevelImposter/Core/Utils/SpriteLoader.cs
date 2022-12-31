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
            LoadSprite(b64, (spriteArr, frameTimes) =>
            {
                // Handle Error
                if (spriteArr.Length <= 0)
                {
                    LILogger.Warn($"Error loading sprite for {element}");
                    return;
                }

                if (spriteArr.Length >= 2) // Animated GIF
                {
                    GIFAnimator gifAnimator = obj.AddComponent<GIFAnimator>();
                    gifAnimator.Init(element, spriteArr, frameTimes);
                    LILogger.Info($"Done loading animated sprite for {element}");
                }
                else // Still Image
                {
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = spriteArr[0];
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
        public void LoadSprite(string b64Image, Action<Sprite[], float[]> onLoad)
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
        public void LoadSprite(byte[] imgBytes, Action<Sprite[], float[]> onLoad)
        {
            if (LIShipStatus.Instance == null)
            {
                LILogger.Error("Cannot load sprite, LIShipStatus.Instance is null");
                return;
            }
            StartCoroutine(CoLoadElement(imgBytes, onLoad).WrapToIl2Cpp());
        }

        [HideFromIl2Cpp]
        private IEnumerator CoLoadElement(byte[] imgBytes, Action<Sprite[], float[]> onLoad)
        {
            _renderCount++;
            Task<TextureMetadata[]> task = Task.Run(() => {
                return ProcessImage(imgBytes);
            });
            while (!task.IsCompleted)
                yield return null;
            TextureMetadata[] texDataArr = task.Result;
            Sprite[] spriteArr = new Sprite[texDataArr.Length];
            float[] frameTimeArr = new float[texDataArr.Length]; 
            for (int i = 0; i < texDataArr.Length; i++)
            {
                while (_renderTimer.ElapsedMilliseconds > (1000 / 15)) // Stay above ~15fps
                    yield return null;
                TextureMetadata texData = texDataArr[i];
                LoadImage(texData);
                spriteArr[i] = texData.sprite;
                frameTimeArr[i] = texData.frameTime;
            }
            onLoad.Invoke(spriteArr, frameTimeArr);
            _renderCount--;
        }

        [HideFromIl2Cpp]
        private TextureMetadata[] ProcessImage(byte[] imgBytes)
        {
            // Bytes to FreeImage
            IntPtr texMemory = FreeImage.FreeImage_OpenMemory(
                Marshal.UnsafeAddrOfPinnedArrayElement(imgBytes, 0),
                (uint)imgBytes.Length
            );
            FREE_IMAGE_FORMAT imageFormat = FreeImage.FreeImage_GetFileTypeFromMemory(
                texMemory,
                imgBytes.Length
            );

            if (imageFormat == FREE_IMAGE_FORMAT.FIF_GIF)
            {
                // Get Handle
                IntPtr multiTexHandle = FreeImage.FreeImage_LoadMultiBitmapFromMemory(
                    imageFormat,
                    texMemory,
                    // TODO: Replace GIF_PLAYBACK because it is O(n^2)
                    (int)FREE_IMAGE_LOAD_FLAGS.GIF_PLAYBACK
                );

                // Iterate
                int pageCount = FreeImage.FreeImage_GetPageCount(multiTexHandle);
                TextureMetadata[] texDataArr = new TextureMetadata[pageCount];
                for (int page = 0; page < pageCount; page++)
                {
                    IntPtr texHandle = FreeImage.FreeImage_LockPage(multiTexHandle, page);
                    TextureMetadata texData = TextureHandleToMetadata(texHandle);
                    texDataArr[page] = texData;

                    // Get Frame Time
                    FreeImage.FreeImage_GetMetadata(
                        FREE_IMAGE_MDMODEL.FIMD_ANIMATION,
                        texHandle,
                        "FrameTime",
                        out IntPtr tag
                    );
                    IntPtr frameTimePtr = FreeImage.FreeImage_GetTagValue(tag);
                    int frameTime = Marshal.ReadInt32(frameTimePtr);
                    texData.frameTime = frameTime / 1000.0f;

                    FreeImage.FreeImage_UnlockPage(multiTexHandle, texHandle, false);
                }

                // Unload
                FreeImage.FreeImage_CloseMultiBitmap(multiTexHandle, 0);
                FreeImage.FreeImage_CloseMemory(texMemory);
                return texDataArr;
            }
            else
            {
                // Get Handle
                IntPtr texHandle = FreeImage.FreeImage_LoadFromMemory(
                    imageFormat,
                    texMemory,
                    0
                );

                // Get Texture
                TextureMetadata[] texDataArr = new TextureMetadata[1];
                texDataArr[0] = TextureHandleToMetadata(texHandle);

                // Unload
                FreeImage.FreeImage_Unload(texHandle);
                FreeImage.FreeImage_CloseMemory(texMemory);
                return texDataArr;
            }
        }

        private TextureMetadata TextureHandleToMetadata(IntPtr texHandle)
        {
            uint texWidth = FreeImage.FreeImage_GetWidth(texHandle);
            uint texHeight = FreeImage.FreeImage_GetHeight(texHandle);
            uint size = texWidth * texHeight * 4;
            byte[] texBytes = new byte[size];
            FreeImage.FreeImage_ConvertToRawBits(
                Marshal.UnsafeAddrOfPinnedArrayElement(texBytes, 0),
                texHandle,
                (int)texWidth * 4,
                32,
                0,
                0,
                0,
                false
            );

            return new TextureMetadata()
            {
                width = (int)texWidth,
                height = (int)texHeight,
                texBytes = texBytes
            };
        }

        [HideFromIl2Cpp]
        private TextureMetadata LoadImage(TextureMetadata texData)
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

            // Garbage Collection
            if (LIShipStatus.Instance != null)
                LIShipStatus.Instance.AddMapTexture(texture);

            // Generate Sprite
            texData.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f,
                0,
                SpriteMeshType.FullRect
            );

            return texData;
        }

        /// <summary>
        /// Metadata to store and send texture data
        /// </summary>
        private class TextureMetadata
        {
            public int width = 0;
            public int height = 0;
            public byte[] texBytes = Array.Empty<byte>();
            public float frameTime = 0; // ms
            public Sprite sprite;
        }
    }
}
