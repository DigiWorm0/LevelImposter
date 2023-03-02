using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace LevelImposter.Core
{
    /// <summary>
    /// A wrapper around the ImageSharp library
    /// </summary>
    public static class ImageSharpWrapper
    {
        public const string DLL_NAME = "SixLabors.ImageSharp.dll";

        private static bool? _isInstalled = null;
        public static bool IsInstalled
        {
            get
            {
                if (_isInstalled != null)
                    return (bool)_isInstalled;
                string gameDir = Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
                string imgSharpDir = Path.Combine(Path.GetDirectoryName(gameDir) ?? "/", DLL_NAME);
                _isInstalled = File.Exists(imgSharpDir);
                return (bool)_isInstalled;
            }
        }

        /// <summary>
        /// Loads texture metadata from array
        /// </summary>
        /// <param name="imgStream">Stream of bytes representing image data</param>
        /// <param name="textureList">Output texture metadata</param>
        /// <returns>TRUE on success</returns>
        public static TextureMetadata[]? LoadImage(byte[] imgData)
        {
            try
            {
                // Load Image & Formats
                using (var image = Image.Load<Rgba32>(imgData, out IImageFormat? imgFormat))
                {
                    // Null Check
                    if (image == null || imgFormat == null)
                        return null;

                    // Iterate Frames
                    var texMetadataArr = new TextureMetadata[image.Frames.Count];
                    for (int i = 0; i < image.Frames.Count; i++)
                    {
                        texMetadataArr[i] = GetTextureMetadata(image, i);
                    }
                    return texMetadataArr;
                }
            }
            catch (Exception e)
            {
                LILogger.Warn(e);
                return null;
            }
        }

        /// <summary>
        /// Loads frame from image into TextureMetadata
        /// </summary>
        /// <param name="image">ImageSharp Image</param>
        /// <param name="index">Frame index</param>
        /// <returns>TextureMetadata with frame data</returns>
        private static TextureMetadata GetTextureMetadata(Image<Rgba32> image, int index)
        {
            var frame = image.Frames[index];
            var frameImg = image.Frames.CloneFrame(index);

            // Get GIF Frame Delay
            GifFrameMetadata gifMetadata = frame.Metadata.GetGifMetadata();
            float frameDelay = gifMetadata.FrameDelay / 100.0f;

            // Populate Metadata
            using (var frameStream = new MemoryStream())
            {
                frameImg.SaveAsPng(frameStream);
                return new()
                {
                    FrameDelay = frameDelay,
                    FrameData = frameStream.ToArray()
                };
            }
        }

        /// <summary>
        /// Metadata to store and send texture data
        /// </summary>
        public struct TextureMetadata
        {
            public TextureMetadata() { }
            public float FrameDelay = 0;
            public byte[] FrameData = Array.Empty<byte>();
        }
    }
}
