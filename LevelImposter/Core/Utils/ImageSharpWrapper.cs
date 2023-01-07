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
        public static TextureMetadata[]? LoadImage(MemoryStream imgStream)
        {
            try
            {
                // Load Image & Formats
                var imgFormat = Image.DetectFormat(imgStream);
                using (var image = Image.Load<Rgba32>(imgStream))
                {
                    // Null Check
                    if (image == null || imgFormat == null)
                        return null;

                    bool isGif = imgFormat.DefaultMimeType == "image/gif";
                    image.Mutate(t => t.Flip(FlipMode.Vertical)); // Fix bug where image is flipped
                    var texMetadataArr = new TextureMetadata[image.Frames.Count];

                    // Iterate Frames
                    for (int i = 0; i < image.Frames.Count; i++)
                    {
                        var frame = image.Frames[i];

                        // Get GIF Frame Delay
                        float frameDelay = 0;
                        if (isGif)
                        {
                            GifFrameMetadata metadata = frame.Metadata.GetGifMetadata();
                            frameDelay = metadata.FrameDelay / 100.0f;
                        }

                        // Populate Metadata
                        texMetadataArr[i] = new()
                        {
                            Width = frame.Width,
                            Height = frame.Height,
                            RawTextureData = new byte[frame.Width * frame.Height * 4],
                            FrameDelay = frameDelay
                        };
                        frame.CopyPixelDataTo(texMetadataArr[i].RawTextureData);
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
            public int Width = 0;
            public int Height = 0;
            public byte[] RawTextureData = Array.Empty<byte>();
        }
    }
}
