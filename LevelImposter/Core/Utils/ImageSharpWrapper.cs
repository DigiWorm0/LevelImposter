using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Diagnostics;

namespace LevelImposter.Core
{
    public static class ImageSharpWrapper
    {
        /// <summary>
        /// Loads texture metadata from array
        /// </summary>
        /// <param name="imgStream">Stream of bytes representing image data</param>
        /// <param name="textureList">Output texture metadata</param>
        /// <returns>TRUE on success</returns>
        public static TextureList LoadImage(MemoryStream imgStream)
        {
            using (var image = Image.Load<Rgba32>(imgStream, out IImageFormat format))
            {
                image.Mutate(t => t.Flip(FlipMode.Vertical));
                if (format.DefaultMimeType == "image/gif")
                {
                    return LoadGIF(image);
                }
                else
                {
                    return LoadImage(image);
                }
            }
        }

        /// <summary>
        /// Loads a still image from memory
        /// </summary>
        /// <param name="image">Image object to load in</param>
        /// <returns><c>TextureList</c> with a singular texture</returns>
        private static TextureList LoadImage(Image<Rgba32> image)
        {
            // Output Metadata
            byte[] buffer = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(buffer);

            TextureMetadata texData = new()
            {
                width = image.Width,
                height = image.Height,
                texStream = new MemoryStream(buffer),
            };

            // Add to List
            TextureList textureList = new();
            textureList.texDataArr = new TextureMetadata[1];
            textureList.texDataArr[0] = texData;

            return textureList;
        }

        /// <summary>
        /// Loads a GIF image from memory
        /// </summary>
        /// <param name="image">Image object to load in</param>
        /// <returns><c>TextureList</c> with cooresponding frame times and raw image bytes</returns>
        private static TextureList LoadGIF(Image<Rgba32> image)
        {
            TextureList textureList = new();
            textureList.texDataArr = new TextureMetadata[image.Frames.Count];
            textureList.frameTimeArr = new float[image.Frames.Count];

            for (int i = 0; i < image.Frames.Count; i++)
            {
                ImageFrame<Rgba32> frame = image.Frames[i];

                // Frame Data
                byte[] buffer = new byte[frame.Width * frame.Height * 4];
                frame.CopyPixelDataTo(buffer);

                // Frame Delay
                GifFrameMetadata metadata = frame.Metadata.GetGifMetadata();
                textureList.frameTimeArr[i] = metadata.FrameDelay / 100.0f;
                textureList.texDataArr[i] = new()
                {
                    width = frame.Width,
                    height = frame.Height,
                    texStream = new MemoryStream(buffer),
                };
            }
            return textureList;
        }

        /// <summary>
        /// Metadata to store and send texture data
        /// </summary>
        public class TextureMetadata
        {
            public int width = 0;
            public int height = 0;
            public MemoryStream texStream;
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
