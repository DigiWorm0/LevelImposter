using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Represents a GIF file.
    /// </summary>
    public class GIFFile : IDisposable
    {
        private static readonly Color[] DEFAULT_COLOR_TABLE = new Color[]
        {
            new Color(0, 0, 0, 0),
            new Color(1, 1, 1, 1)
        };

        public bool IsLoaded { get; private set; }

        // LZW Decoder
        private ushort[][]? _codeTable = null; // Table of "code"s to color indexes
        private Color[]? _pixelBuffer = null; // Buffer of pixel colors

        // Graphic Control Extension
        private GIFGraphicsControl? _lastGraphicsControl { get; set; }

        // Logical Screen Descriptor
        private Color[] _globalColorTable = DEFAULT_COLOR_TABLE; // Table of indexes to colors
        private bool _hasGlobalColorTable = false; // True if there is a global color table
        private int _globalColorTableSize = 0; // Size of the global color table
        private ushort _backgroundColorIndex = 0; // Index of the background color in the global color table
        private Color _backgroundColor = Color.clear; // Background color

        // Image Descriptor
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public List<GIFFrame> Frames { get; private set; }

        /// <summary>
        /// Loads the GIF file from a given stream.
        /// </summary>
        /// <param name="dataStream">Stream of raw GIF data</param>
        public void Load(Stream dataStream)
        {
            using (var reader = new BinaryReader(dataStream))
            {
                IsLoaded = false;
                ReadHeader(reader);
                ReadDescriptor(reader);
                ReadGlobalColorTable(reader);
                while (ReadBlock(reader)) { }
                _pixelBuffer = null;
                _codeTable = null;
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Gets the sprite of a frame. Renders the frame if it hasn't been rendered yet.
        /// </summary>
        /// <param name="frameIndex">Index of the frame</param>
        /// <returns>The sprite of the frame</returns>
        public Sprite GetFrameSprite(int frameIndex)
        {
            if (!IsLoaded)
                throw new Exception("GIF file is not loaded");
            if (frameIndex < 0 || frameIndex >= Frames.Count)
                throw new IndexOutOfRangeException("Frame index out of range");

            var frame = Frames[frameIndex];
            if (!frame.IsRendered)
                RenderFrame(frameIndex);
            if (frame.RenderedSprite == null)
                throw new Exception("Frame sprite is null");
            return frame.RenderedSprite;
        }

        /// <summary>
        /// Destroys the GIF file and frees up memory.
        /// </summary>
        public void Dispose()
        {
            _pixelBuffer = null;
            _codeTable = null;
            foreach (var frame in Frames)
            {
                if (frame.RenderedSprite?.texture != null)
                    UnityEngine.Object.Destroy(frame.RenderedSprite.texture);
                if (frame.RenderedSprite != null)
                    UnityEngine.Object.Destroy(frame.RenderedSprite);
                frame.IndexStream = null;
            }
        }

        /// <summary>
        /// Verifies the GIF file header.
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        private void ReadHeader(BinaryReader reader)
        {
            // Header
            bool isGIF = new string(reader.ReadChars(3)) == "GIF";
            if (!isGIF)
                throw new Exception("File is not a GIF");

            // Version
            string version = new string(reader.ReadChars(3));
            if (version != "89a" && version != "87a")
                throw new Exception("File is not a GIF89a or GIF87a");
        }

        /// <summary>
        /// Retrieves the metadata of the GIF file.
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        /// <param name="gifData">The GIFData to store the metadata in</param>
        private void ReadDescriptor(BinaryReader reader)
        {
            // Logical Screen Descriptor
            ushort width = reader.ReadUInt16();
            ushort height = reader.ReadUInt16();

            byte packedField = reader.ReadByte();
            bool hasGlobalColorTable = (packedField & 0b10000000) != 0;
            //int colorResolution = ((packedField & 0b01110000) >> 4) + 1;
            //bool sortFlag = (packedField & 0b00001000) != 0;
            int globalColorTableSize = 1 << ((packedField & 0b00000111) + 1);

            byte backgroundColorIndex = reader.ReadByte();
            reader.ReadByte(); // Pixel Aspect Ratio

            // GIFData
            _hasGlobalColorTable = hasGlobalColorTable;
            _globalColorTableSize = globalColorTableSize;
            _backgroundColorIndex = backgroundColorIndex;

            Width = width;
            Height = height;
            Frames = new List<GIFFrame>();
        }

        /// <summary>
        /// Reads the global color table from the GIF file.
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        /// <param name="gifData">GIFData to store color data</param>
        private void ReadGlobalColorTable(BinaryReader reader)
        {
            if (!_hasGlobalColorTable)
                return;

            // Global Color Table
            var globalColorTable = new Color[_globalColorTableSize];
            for (int i = 0; i < _globalColorTableSize; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();

                globalColorTable[i] = new Color(r / 255f, g / 255f, b / 255f);
            }
            _globalColorTable = globalColorTable;
        }

        /// <summary>
        /// Reads a block of unknown data from the GIF file.
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        /// <param name="gifData">GIFData to store block data</param>\
        /// <returns><c>true</c> if the block was read successfully, <c>false</c> if the end of the file was reached</returns>
        private bool ReadBlock(BinaryReader reader)
        {
            byte blockType = reader.ReadByte();
            switch (blockType)
            {
                case 0x21:
                    ReadExtension(reader);
                    return true;
                case 0x2C:
                    ReadImageBlock(reader);
                    return true;
                case 0x3B:
                    // End of File
                    return false;
                default:
                    throw new Exception("Invalid block type " + blockType);
            }
        }

        /// <summary>
        /// Reads an extension block from the GIF file.
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        /// <param name="gifData">GIFData to store extension data</param>
        private void ReadExtension(BinaryReader reader)
        {
            byte extensionLabel = reader.ReadByte();
            switch (extensionLabel)
            {
                case 0xF9: // Graphic Control Extension

                    // Block Size
                    byte blockSize = reader.ReadByte();
                    if (blockSize != 4)
                        throw new Exception("Invalid block size " + blockSize);

                    byte packedField = reader.ReadByte();
                    var disposalMethod = (FrameDisposalMethod)((packedField & 0b00011100) >> 2);
                    bool transparentColorFlag = (packedField & 0b00000001) != 0;
                    float delay = reader.ReadUInt16() / 100f;
                    byte transparentColorIndex = reader.ReadByte();

                    // Block Terminator
                    byte blockTerminator = reader.ReadByte();
                    if (blockTerminator != 0)
                        throw new Exception("Invalid block terminator " + blockTerminator);

                    // GIFGraphicsControl
                    _lastGraphicsControl = new GIFGraphicsControl()
                    {
                        Delay = delay,
                        DisposalMethod = disposalMethod,
                        TransparentColorFlag = transparentColorFlag,
                        TransparentColorIndex = transparentColorIndex
                    };

                    break;
                case 0xFF: // Application Extension
                case 0xFE: // Comment Extension
                case 0x01: // Plain Text Extension
                    while (true)
                    {
                        byte subBlockSize = reader.ReadByte();
                        if (subBlockSize == 0)
                            break;
                        reader.ReadBytes(subBlockSize); // Skip Over Data
                    }
                    break;
                default:
                    throw new Exception("Invalid extension label " + extensionLabel);
            }
        }

        /// <summary>
        /// Reads an image block from the GIF file.
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        /// <param name="gifData">GIFData to store image data</param>\
        private void ReadImageBlock(BinaryReader reader)
        {
            // Image Descriptor
            ushort imageLeftPosition = reader.ReadUInt16();
            ushort imageTopPosition = reader.ReadUInt16();
            ushort imageWidth = reader.ReadUInt16();
            ushort imageHeight = reader.ReadUInt16();

            byte packedField = reader.ReadByte();
            bool hasLocalColorTable = (packedField & 0b10000000) != 0;
            bool interlaceFlag = (packedField & 0b01000000) != 0;
            bool sortFlag = (packedField & 0b00100000) != 0;
            int localColorTableSize = 1 << ((packedField & 0b00000111) + 1);

            if (interlaceFlag)
                throw new NotImplementedException("Interlaced GIFs are not implemented");

            // Local Color Table
            var localColorTable = new Color[localColorTableSize];
            if (hasLocalColorTable)
            {
                for (int i = 0; i < localColorTableSize; i++)
                {
                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();

                    Color color = new Color(r / 255f, g / 255f, b / 255f);
                    localColorTable[i] = color;
                }
            }

            // Image Data
            byte minCodeSize = reader.ReadByte();

            // Read Blocks
            List<byte> byteStream = new List<byte>();
            while (true)
            {
                byte subBlockSize = reader.ReadByte(); // Read Sub Block
                if (subBlockSize == 0) // End of Image Data
                    break;

                // Read Sub Block Data
                byte[] subBlockData = reader.ReadBytes(subBlockSize);
                byteStream.AddRange(subBlockData);
            }

            var indexStream = DecodeLZW(byteStream.ToArray(), minCodeSize, imageWidth * imageHeight);

            // GIFFrame
            GIFFrame frame = new GIFFrame()
            {
                GraphicsControl = _lastGraphicsControl,
                HasLocalColorTable = hasLocalColorTable,
                LocalColorTable = localColorTable,
                InterlaceFlag = interlaceFlag,
                SortFlag = sortFlag,

                LeftPosition = imageLeftPosition,
                TopPosition = imageTopPosition,
                Width = imageWidth,
                Height = imageHeight,

                IndexStream = indexStream
            };
            Frames.Add(frame);

            _lastGraphicsControl = null;
        }

        /// <summary>
        /// Decodes the LZW encoded image data of a GIF
        /// </summary>
        /// <param name="byteBuffer">Raw bytes from the image block</param>
        /// <param name="minCodeSize">Minimum code size in bits</param>
        /// <param name="expectedSize">Expected size of the index stream</param>
        /// <returns>List of color indices</returns>
        private List<ushort> DecodeLZW(byte[] byteBuffer, byte minCodeSize, int expectedSize)
        {
            BitArray bitBuffer = new BitArray(byteBuffer);  // The raw data as a bit array
            int clearCode = 1 << minCodeSize; // Code used to clear the code table
            int endOfInformationCode = clearCode + 1; // Code used to signal the end of the image data

            var codeTableIndex = endOfInformationCode + 1; // The next index in the code table
            int codeSize = minCodeSize + 1; // The size of the codes in bits
            int previousCode = -1; // The previous code

            var indexStream = new List<ushort>(1 << 12); // The index stream

            // Initialize Code Table
            if (_codeTable == null)
                _codeTable = new ushort[1 << 12][];
            for (ushort k = 0; k < codeTableIndex; k++)
                _codeTable[k] = k < clearCode ? new ushort[] { k } : new ushort[0];

            // Decode LZW
            int i = 0;
            while (i + codeSize < bitBuffer.Length)
            {
                // Read Code
                int code = 0;
                for (int j = 0; j < codeSize; j++)
                    code |= bitBuffer[i + j] ? 1 << j : 0;
                i += codeSize;

                // Special Codes
                if (code == clearCode)
                {
                    // Reset LZW
                    codeTableIndex = endOfInformationCode + 1;
                    codeSize = minCodeSize + 1;
                    previousCode = -1;
                    continue;
                }
                else if (code == endOfInformationCode)
                {
                    // End of Information
                    break;
                }
                else if (previousCode == -1)
                {
                    // Initial Code
                    indexStream.AddRange(_codeTable[code]);
                    previousCode = code;
                    continue;
                }

                // Compare to Code Table
                if (code < codeTableIndex)
                {
                    // Get New Code
                    var currentCodeArray = _codeTable[code];
                    var previousCodeArray = _codeTable[previousCode];
                    var newCode = new ushort[previousCodeArray.Length + 1];
                    previousCodeArray.CopyTo(newCode, 0);
                    newCode[newCode.Length - 1] = currentCodeArray[0];

                    // Add to Index Stream
                    indexStream.AddRange(currentCodeArray);

                    // Add to Code Table
                    if (codeTableIndex < _codeTable.Length)
                        _codeTable[codeTableIndex] = newCode;
                    codeTableIndex++;
                }
                else
                {
                    // Get New Code
                    var previousCodeArray = _codeTable[previousCode];
                    var newCode = new ushort[previousCodeArray.Length + 1];
                    previousCodeArray.CopyTo(newCode, 0);
                    newCode[newCode.Length - 1] = previousCodeArray[0];

                    // Add to Index Stream
                    indexStream.AddRange(newCode);

                    // Add to Code Table
                    if (codeTableIndex < _codeTable.Length)
                        _codeTable[codeTableIndex] = newCode;
                    codeTableIndex++;
                }

                // Update Previous Code
                previousCode = code;

                // Increase Code Size
                if (codeTableIndex >= (1 << codeSize) && codeSize < 12)
                    codeSize++;
            }

            // Fill in the rest of the index stream
            while (indexStream.Count < expectedSize)
                indexStream.Add(0);

            return indexStream;
        }

        /// <summary>
        /// Pre-renders all frames of the GIF. Requires the GIF to be loaded.
        /// </summary>
        public void RenderAllFrames()
        {
            RenderFrame(Frames.Count - 1);
        }

        /// <summary>
        /// Renders a frame of the GIF. Requires the GIF to be loaded. Will result in all previous frames being rendered as well.
        /// </summary>
        /// <param name="targetFrame">The frame to render</param>
        public void RenderFrame(int frameIndex)
        {
            if (!IsLoaded)
                throw new Exception("GIF is not loaded");
            if (frameIndex < 0 || frameIndex >= Frames.Count)
                throw new Exception($"Frame index {frameIndex} is out of range");

            // Create pixel buffer
            if (_pixelBuffer == null)
            {
                _pixelBuffer = new Color[Width * Height];
                for (int i = 0; i < _pixelBuffer.Length; i++)
                    _pixelBuffer[i] = _backgroundColor;
            }

            // Render all frames up to the target frame
            for (int i = 0; i <= frameIndex; i++)
            {
                // Frame
                var frame = Frames[i];
                if (frame.IsRendered) // Skip rendered frames
                    continue;
                if (frame.IndexStream == null)
                    throw new Exception($"Frame {i} index stream is null");
                var graphicsControl = frame.GraphicsControl;

                // Create temp pixel buffer
                Color[]? tempBuffer = null;
                if (frame.DisposalMethod == FrameDisposalMethod.RestoreToPrevious)
                {
                    tempBuffer = new Color[_pixelBuffer.Length];
                    _pixelBuffer.CopyTo(tempBuffer, 0);
                }

                // Create frame texture
                bool pixelArtMode = LIShipStatus.Instance?.CurrentMap?.properties.pixelArtMode == true;
                var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave,
                    requestedMipmapLevel = 0
                };

                // Get frame data
                var colorTable = frame.HasLocalColorTable ? frame.LocalColorTable : _globalColorTable;
                var x = frame.LeftPosition;
                var y = frame.TopPosition;
                var w = frame.Width;
                var h = frame.Height;

                // Loop through pixels
                for (int o = 0; o < w * h; o++)
                {
                    var colorIndex = frame.IndexStream[o];

                    // Skip transparent pixels
                    if (graphicsControl != null &&
                        graphicsControl.TransparentColorFlag &&
                        colorIndex == graphicsControl.TransparentColorIndex)
                    {
                        continue;
                    }

                    // Calculate pixel index based on frame position
                    int newX = o % w;
                    int newY = o / w;
                    int pixelIndex = (Height - 1 - (y + newY)) * Width + (x + newX);

                    // Set pixel color
                    var color = colorTable[colorIndex];
                    _pixelBuffer[pixelIndex] = color;
                }

                // Apply Texture
                texture.SetPixels(_pixelBuffer);
                texture.Apply();

                // Handle frame disposal
                switch (frame.DisposalMethod)
                {
                    case FrameDisposalMethod.RestoreToPrevious:
                        // Restore previous buffer
                        if (tempBuffer != null)
                            _pixelBuffer = tempBuffer;
                        break;
                    case FrameDisposalMethod.RestoreToBackgroundColor:
                        // Fill pixel buffer with background color
                        for (int o = 0; o < w * h; o++)
                        {
                            // Calculate pixel index based on frame position
                            int newX = o % w;
                            int newY = o / w;
                            int pixelIndex = (Height - 1 - (y + newY)) * Width + (x + newX);

                            // Set pixel color
                            _pixelBuffer[pixelIndex] = _backgroundColor;
                        }
                        break;
                    case FrameDisposalMethod.NoDisposal:
                    case FrameDisposalMethod.DoNotDispose:
                        // Do nothing
                        break;
                }

                // Generate Sprite
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100.0f,
                    0,
                    SpriteMeshType.FullRect
                );
                sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;

                // Apply to frame
                frame.IndexStream = null; // Free memory
                frame.RenderedSprite = sprite;
            }
        }

        /// <summary>
        /// Represents data for graphics control
        /// </summary>
        public class GIFGraphicsControl
        {
            public float Delay { get; set; } // seconds
            public FrameDisposalMethod DisposalMethod { get; set; }
            public bool TransparentColorFlag { get; set; }
            public int TransparentColorIndex { get; set; }
        }

        /// <summary>
        /// Represents the data needed to draw a single frame of a GIF.
        /// </summary>
        public class GIFFrame
        {
            // Graphic Control Extension
            public GIFGraphicsControl? GraphicsControl { get; set; }

            public float Delay { get { return GraphicsControl?.Delay ?? 0; } }
            public FrameDisposalMethod DisposalMethod { get { return GraphicsControl?.DisposalMethod ?? FrameDisposalMethod.DoNotDispose; } }
            public bool IsRendered { get { return RenderedSprite != null; } }

            // Image Descriptor
            public Color[]? LocalColorTable { get; set; }
            public bool HasLocalColorTable { get; set; }
            public bool InterlaceFlag { get; set; }
            public bool SortFlag { get; set; }

            public int LeftPosition { get; set; }
            public int TopPosition { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public List<ushort>? IndexStream { get; set; }
            public Sprite? RenderedSprite { get; set; }

        }

        /// <summary>
        /// The disposal method for a GIF frame.
        /// </summary>
        public enum FrameDisposalMethod
        {
            NoDisposal = 0,
            DoNotDispose = 1,
            RestoreToBackgroundColor = 2,
            RestoreToPrevious = 3
        }
    }
}