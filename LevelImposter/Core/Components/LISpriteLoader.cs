using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    public class LISpriteLoader : MonoBehaviour
    {
        public LISpriteLoader(IntPtr intPtr) : base(intPtr)
        {
        }

        private static bool _shouldRender = true;
        private static List<LISpriteLoader> _activeRenders = new();

        public static int RenderCount => _activeRenders.Count;

        private readonly UnityEvent<Sprite> _onLoad = new();
        private LIElement? _element;
        private Thread? _processThread;
        private byte[] _texBytes = Array.Empty<byte>();
        private uint _texWidth = 1;
        private uint _texHeight = 1;
        private bool _isReady = false;

        public UnityEvent<Sprite> OnLoad => _onLoad;

        /// <summary>
        /// Loads the custom sprite on an LIElement
        /// </summary>
        /// <param name="element">LIElement to read</param>
        [HideFromIl2Cpp]
        public void LoadElement(LIElement element)
        {
            LILogger.Info($"Loading sprite for {element}");
            StopAllCoroutines();
            StartCoroutine(CoLoadElement(element).WrapToIl2Cpp());
        }

        [HideFromIl2Cpp]
        private IEnumerator CoLoadElement(LIElement element)
        {
            _activeRenders.Add(this);
            _element = element;
            _processThread = new Thread(ProcessImage);
            _processThread.Start();
            while (!_isReady || !_shouldRender)
                yield return null;
            _shouldRender = false;
            LoadImage();
        }

        public void OnDestroy()
        {
            _activeRenders.Remove(this);
        }
        public void LateUpdate()
        {
            _shouldRender = true;
        }

        /// <summary>
        /// Thread-safe image processer
        /// </summary>
        private void ProcessImage()
        {
            _isReady = false;

            // Get Image Bytes
            string imgString = _element?.properties.spriteData ?? "";
            byte[] imgBytes = MapUtils.ParseBase64(imgString);

            // Bytes to FreeImage
            IntPtr texMemory = FreeImage.FreeImage_OpenMemory(
                Marshal.UnsafeAddrOfPinnedArrayElement(imgBytes, 0),
                (uint)imgBytes.Length
            );
            FREE_IMAGE_FORMAT imageFormat = FreeImage.FreeImage_GetFileTypeFromMemory(
                texMemory,
                imgBytes.Length
            );
            IntPtr texHandle = FreeImage.FreeImage_LoadFromMemory(
                imageFormat,
                texMemory,
                0
            );

            // FreeImage to Texture Bytes
            _texWidth = FreeImage.FreeImage_GetWidth(texHandle);
            _texHeight = FreeImage.FreeImage_GetHeight(texHandle);
            uint size = _texWidth * _texHeight * 4;
            _texBytes = new byte[size];
            FreeImage.FreeImage_ConvertToRawBits(
                Marshal.UnsafeAddrOfPinnedArrayElement(_texBytes, 0),
                texHandle,
                (int)_texWidth * 4,
                32,
                0,
                0,
                0,
                false
            );

            // Release Pointers
            if (texHandle != IntPtr.Zero)
                FreeImage.FreeImage_Unload(texHandle);

            _isReady = true;
        }

        /// <summary>
        /// Main-thread image loader
        /// </summary>
        private void LoadImage()
        {
            // Generate Texture
            bool pixelArtMode = LIShipStatus.Instance.CurrentMap.properties.pixelArtMode == true;
            Texture2D texture = new(
                (int)_texWidth,
                (int)_texHeight,
                TextureFormat.BGRA32,
                1,
                false
            )
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
            };
            texture.LoadRawTextureData(_texBytes);
            texture.Apply(false, true);
            LIShipStatus.Instance.AddMapTexture(texture);

            // Generate Sprite
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f,
                0,
                SpriteMeshType.FullRect
            );

            // Apply Sprite
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            // Fire Events
            LILogger.Info($"Done loading sprite for {_element}");
            Destroy(this);
            _onLoad.Invoke(sprite);
        }
    }
}
