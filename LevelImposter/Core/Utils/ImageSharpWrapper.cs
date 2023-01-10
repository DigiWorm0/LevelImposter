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
    /// <summary>
    /// A wrapper around the ImageSharp library
    /// </summary>
    public static class ImageSharpWrapper
    {
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
                        var frame = image.Frames[i];
                        var frameImg = image.Frames.CloneFrame(i);

                        // Get GIF Frame Delay
                        GifFrameMetadata gifMetadata = frame.Metadata.GetGifMetadata();
                        float frameDelay = gifMetadata.FrameDelay / 100.0f;

                        // Populate Metadata
                        using (var frameStream = new MemoryStream())
                        {
                            frameImg.SaveAsPng(frameStream);
                            texMetadataArr[i] = new()
                            {
                                FrameDelay = frameDelay,
                                FrameData = frameStream.ToArray()
                            };
                        }
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
