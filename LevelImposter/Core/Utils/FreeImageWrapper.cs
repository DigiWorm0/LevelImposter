using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Core
{
    public static class FreeImageWrapper
    {
        /// <summary>
        /// Loads texture metadata from array
        /// </summary>
        /// <param name="imgBytes">Array of bytes representing image data</param>
        /// <param name="textureList">Output texture metadata</param>
        /// <returns>TRUE on success</returns>
        public static bool LoadImage(byte[] imgBytes, out TextureList textureList)
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

            bool isSuccess;
            if (imageFormat == FREE_IMAGE_FORMAT.FIF_GIF)
            {
                isSuccess = LoadGIF(texMemory, out textureList);
            }
            else
            {
                isSuccess = LoadImage(texMemory, imageFormat, out textureList);
            }

            FreeImage.FreeImage_CloseMemory(texMemory);
            return isSuccess;
        }

        /// <summary>
        /// Loads a still image from memory
        /// </summary>
        /// <param name="texMemory">Pointer to Texture Data</param>
        /// <param name="imageFormat">Image format to import as</param>
        /// <param name="textureList">Texture list that is returned</param>
        /// <returns></returns>
        private static bool LoadImage(IntPtr texMemory, FREE_IMAGE_FORMAT imageFormat, out TextureList textureList)
        {
            textureList = new TextureList();

            IntPtr texHandle = FreeImage.FreeImage_LoadFromMemory(
                imageFormat,
                texMemory,
                0
            );
            if (texHandle == IntPtr.Zero)
                return false;

            // Get Texture
            TextureMetadata texData = TextureHandleToMetadata(texHandle);
            textureList.texDataArr = new TextureMetadata[] { texData };

            // Unload
            FreeImage.FreeImage_Unload(texHandle);
            return true;
        }

        /// <summary>
        /// Loads GIF metadata from memory
        /// </summary>
        /// <param name="texMemory">Pointer to Texture Data</param>
        /// <param name="textureList">Texture list that is returned</param>
        /// <returns>TRUE on success</returns>
        private static bool LoadGIF(IntPtr texMemory, out TextureList textureList)
        {
            textureList = new TextureList();
            
            // Get Handle
            IntPtr multiTexHandle = FreeImage.FreeImage_LoadMultiBitmapFromMemory(
                FREE_IMAGE_FORMAT.FIF_GIF,
                texMemory,
                // TODO: Replace GIF_PLAYBACK because it is O(n^2)
                (int)FREE_IMAGE_LOAD_FLAGS.GIF_PLAYBACK
            );
            if (multiTexHandle == IntPtr.Zero)
                return false;

            // Iterate
            int pageCount = FreeImage.FreeImage_GetPageCount(multiTexHandle);
            textureList.texDataArr = new TextureMetadata[pageCount];
            textureList.frameTimeArr = new float[pageCount];
            for (int page = 0; page < pageCount; page++)
            {
                IntPtr texHandle = FreeImage.FreeImage_LockPage(multiTexHandle, page);
                TextureMetadata texData = TextureHandleToMetadata(texHandle);
                textureList.texDataArr[page] = texData;

                // Get Frame Time
                bool hasFrameTime = FreeImage.FreeImage_GetMetadata(
                    FREE_IMAGE_MDMODEL.FIMD_ANIMATION,
                    texHandle,
                    "FrameTime",
                    out IntPtr tag
                );
                if (hasFrameTime)
                {
                    IntPtr frameTimePtr = FreeImage.FreeImage_GetTagValue(tag);
                    float frameTime = Marshal.ReadInt32(frameTimePtr) / 1000.0f;
                    textureList.frameTimeArr[page] = frameTime;
                }
                else
                {
                    textureList.frameTimeArr[page] = 0;
                }

                // Release
                FreeImage.FreeImage_UnlockPage(multiTexHandle, texHandle, false);
            }

            // Unload
            FreeImage.FreeImage_CloseMultiBitmap(multiTexHandle, 0);
            return true;
        }

        private static TextureMetadata TextureHandleToMetadata(IntPtr texHandle)
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

        /// <summary>
        /// Metadata to store and send texture data
        /// </summary>
        public class TextureMetadata
        {
            public int width = 0;
            public int height = 0;
            public byte[] texBytes = Array.Empty<byte>();
        }
        public class TextureList
        {
            public float[] frameTimeArr = Array.Empty<float>();
            public TextureMetadata[] texDataArr = Array.Empty<TextureMetadata>();
            public bool isAnimated
            {
                get
                {
                    return texDataArr.Length > 1;
                }
            }
        }
    }
}
