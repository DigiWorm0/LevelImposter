using System;
using System.Collections.Generic;
using System.IO;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.Core.Utils;
using LevelImposter.FileIO.DataBlock;
using LevelImposter.Test;
using UnityEngine;

namespace LevelImposter.AssetLoader.FileContainers;

/// <summary>
///     Represents a GIF file.
/// </summary>
public class GIFFile(string name)
{
    /// <summary>
    ///     The disposal method for a GIF frame.
    /// </summary>
    public enum FrameDisposalMethod
    {
        NoDisposal = 0,
        DoNotDispose = 1,
        RestoreToBackgroundColor = 2,
        RestoreToPrevious = 3
    }

    private static readonly ColorData[] DefaultColorTable =
    {
        new(0, 0, 0, 0),
        new(255, 255, 255, 255)
    };

    // LZW Decoder
    private static readonly ushort[][] CodeTable = new ushort[1 << 12][]; // Table of "code"s to color indexes

    // Memory
    private static MemoryBlock? _outputBuffer;

    // Other Data
    private readonly ColorData _backgroundColor = ColorData.Clear; // Background color
    private GCBehavior? _gcBehavior;

    // Logical Screen Descriptor
    private ColorData[] _globalColorTable = DefaultColorTable; // Table of indexes to colors
    private int _globalColorTableSize; // Size of the global color table
    private bool _hasGlobalColorTable; // True if there is a global color table
    private uint[]? _pixelBuffer;

    // GIF File
    public bool IsLoaded { get; private set; }
    public string Name { get; } = name;
    public Texture2D DefaultTexture => GetFrameSprite(0).texture;

    // Graphic Control Extension
    private GIFGraphicsControl? LastGraphicsControl { get; set; }

    // Image Descriptor
    public ushort Width { get; private set; }
    public ushort Height { get; private set; }
    public List<GIFFrame> Frames { get; private set; } = [];

    /// <summary>
    ///     Checks if the given data is a GIF file.
    /// </summary>
    /// <param name="data">MemoryBlock of raw GIF data</param>
    /// <returns>True if the data is a GIF file. False otherwise</returns>
    public static bool IsGIF(byte[] data)
    {
        return data[0] == 'G' &&
               data[1] == 'I' &&
               data[2] == 'F' &&
               data[3] == '8' &&
               (data[4] == '7' || data[4] == '9') &&
               data[5] == 'a';
    }

    /// <summary>
    ///     Loads the GIF file from a given stream.
    /// </summary>
    /// <param name="dataStream">Stream of raw GIF data</param>
    /// <param name="gcBehavior">Garbage collection behavior for the loaded textures</param>
    public void Load(Stream dataStream, GCBehavior? gcBehavior = null)
    {
#if PROFILING
        using var _ = Profiler.Measure(
            "GIFFile.Load",
            Name);
#endif
        using var reader = new BinaryReader(dataStream);

        IsLoaded = false;
        _gcBehavior = gcBehavior;
        ReadHeader(reader);
        ReadDescriptor(reader);
        ReadGlobalColorTable(reader);
        while (ReadBlock(reader))
        {
        }

        IsLoaded = true;
    }

    /// <summary>
    ///     Gets the sprite of a frame. Renders the frame if it hasn't been rendered yet.
    /// </summary>
    /// <param name="frameIndex">Index of the frame</param>
    /// <returns>The texture of the frame</returns>
    public Sprite GetFrameSprite(int frameIndex)
    {
        if (!IsLoaded)
            throw new Exception("GIF file is not loaded");
        if (frameIndex < 0 || frameIndex >= Frames.Count)
            throw new IndexOutOfRangeException("Frame index out of range");

        var frame = Frames[frameIndex];
        if (!frame.IsRendered)
            RenderFrame(frameIndex);

        return frame.RenderedSprite ?? throw new Exception("Frame sprite is null");
    }

    /// <summary>
    ///     Verifies the GIF file header.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadHeader(BinaryReader reader)
    {
        // Header
        var isGIF = new string(reader.ReadChars(3)) == "GIF";
        if (!isGIF)
            throw new Exception("File is not a GIF");

        // Version
        var version = new string(reader.ReadChars(3));
        if (version != "89a" && version != "87a")
            throw new Exception("File is not a GIF89a or GIF87a");
    }

    /// <summary>
    ///     Retrieves the metadata of the GIF file.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadDescriptor(BinaryReader reader)
    {
        // Logical Screen Descriptor
        var width = reader.ReadUInt16();
        var height = reader.ReadUInt16();

        var packedField = reader.ReadByte();
        var hasGlobalColorTable = (packedField & 0b10000000) != 0;
        //int colorResolution = ((packedField & 0b01110000) >> 4) + 1;
        //bool sortFlag = (packedField & 0b00001000) != 0;
        var globalColorTableSize = 1 << ((packedField & 0b00000111) + 1);

        reader.ReadByte(); // Background Color Index
        reader.ReadByte(); // Pixel Aspect Ratio

        // GIFData
        _hasGlobalColorTable = hasGlobalColorTable;
        _globalColorTableSize = globalColorTableSize;

        Width = width;
        Height = height;
        Frames = [];
    }

    /// <summary>
    ///     Reads the global color table from the GIF file.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadGlobalColorTable(BinaryReader reader)
    {
        if (!_hasGlobalColorTable)
            return;

        // Global Color Table
        _globalColorTable = new ColorData[_globalColorTableSize];
        for (var i = 0; i < _globalColorTableSize; i++)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();

            _globalColorTable[i] = new ColorData(r, g, b, 255);
        }
    }

    /// <summary>
    ///     Reads a block of unknown data from the GIF file.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    /// <returns><c>true</c> if the block was read successfully, <c>false</c> if the end of the file was reached</returns>
    private bool ReadBlock(BinaryReader reader)
    {
        var blockType = reader.ReadByte();
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
    ///     Reads an extension block from the GIF file.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadExtension(BinaryReader reader)
    {
        var extensionLabel = reader.ReadByte();
        switch (extensionLabel)
        {
            case 0xF9: // Graphic Control Extension

                // Block Size
                var blockSize = reader.ReadByte();
                if (blockSize != 4)
                    throw new Exception("Invalid block size " + blockSize);

                var packedField = reader.ReadByte();
                var disposalMethod = (FrameDisposalMethod)((packedField & 0b00011100) >> 2);
                var transparentColorFlag = (packedField & 0b00000001) != 0;
                var delay = reader.ReadUInt16() / 100f;
                var transparentColorIndex = reader.ReadByte();

                // Block Terminator
                var blockTerminator = reader.ReadByte();
                if (blockTerminator != 0)
                    throw new Exception("Invalid block terminator " + blockTerminator);

                // GIFGraphicsControl
                LastGraphicsControl = new GIFGraphicsControl
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
                    var subBlockSize = reader.ReadByte();
                    if (subBlockSize == 0)
                        break;
                    reader.BaseStream.Position += subBlockSize; // Skip Over Data
                }

                break;
            default:
                throw new Exception("Invalid extension label " + extensionLabel);
        }
    }

    /// <summary>
    ///     Reads an image block from the GIF file.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadImageBlock(BinaryReader reader)
    {
        // Image Descriptor
        var imageLeftPosition = reader.ReadUInt16();
        var imageTopPosition = reader.ReadUInt16();
        var imageWidth = reader.ReadUInt16();
        var imageHeight = reader.ReadUInt16();

        var packedField = reader.ReadByte();
        var hasLocalColorTable = (packedField & 0b10000000) != 0;
        var interlaceFlag = (packedField & 0b01000000) != 0;
        var sortFlag = (packedField & 0b00100000) != 0;
        var localColorTableSize = 1 << ((packedField & 0b00000111) + 1);

        if (interlaceFlag)
            throw new NotImplementedException("Interlaced GIFs are not implemented");

        // Local Color Table
        var localColorTable = new ColorData[localColorTableSize];
        if (hasLocalColorTable)
            for (var i = 0; i < localColorTableSize; i++)
            {
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();

                localColorTable[i] = new ColorData(r, g, b, 255);
            }

        // Image Data
        var minCodeSize = reader.ReadByte();

        // Get Block Length
        long byteLength = 0;
        var bytePosition = reader.BaseStream.Position;
        while (true)
        {
            var subBlockSize = reader.ReadByte(); // Read Sub Block
            if (subBlockSize == 0) // End of Image Data
                break;
            byteLength += subBlockSize;
            reader.BaseStream.Position += subBlockSize;
        }

        // Get Block Data
        var byteData = new byte[byteLength];
        reader.BaseStream.Position = bytePosition;
        bytePosition = 0;
        while (true)
        {
            var subBlockSize = reader.ReadByte(); // Read Sub Block
            if (subBlockSize == 0) // End of Image Data
                break;
            bytePosition += reader.Read(byteData, (int)bytePosition, subBlockSize);
        }

        // Decode LZW
        var indexStream = DecodeLZW(byteData, minCodeSize, imageWidth * imageHeight);

        // GIFFrame
        var frame = new GIFFrame
        {
            GraphicsControl = LastGraphicsControl,
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

        LastGraphicsControl = null;
    }

    /// <summary>
    ///     Decodes the LZW encoded image data of a GIF.
    ///     Takes an array of bytes and converts it into a list of codes and then to a list of color indices.
    /// </summary>
    /// <param name="byteBuffer">Raw bytes from the image block</param>
    /// <param name="minCodeSize">Minimum code size in bits</param>
    /// <param name="expectedSize">Expected size of the final index stream</param>
    /// <returns>List of color indices</returns>
    private static List<ushort> DecodeLZW(byte[] byteBuffer, byte minCodeSize, int expectedSize)
    {
        // Initialize LZW Variables
        var clearCode = 1 << minCodeSize; // Code used to clear the code table
        var endOfInformationCode = clearCode + 1; // Code used to signal the end of the image data

        var codeTableIndex = endOfInformationCode + 1; // The next index in the code table
        var codeSize = minCodeSize + 1; // The size of the codes in bits
        var previousCode = -1; // The previous code

        var indexStream = new List<ushort>(expectedSize); // The index stream

        // Initialize Code Table
        for (ushort k = 0; k < codeTableIndex; k++)
            CodeTable[k] = k < clearCode ? new[] { k } : new ushort[0];

        // Decode LZW
        var bitOffset = 0;
        var byteIndex = 0;
        while (byteIndex * 8 + bitOffset + codeSize < byteBuffer.Length * 8)
        {
            // Read code at current byte/bit position
            var code = 0;
            for (var i = 0; i < codeSize; i++)
            {
                // Get Bit
                code |= ((byteBuffer[byteIndex] >> bitOffset) & 1) << i;

                // Increment position
                bitOffset++;
                if (bitOffset == 8)
                {
                    bitOffset = 0;
                    byteIndex++;
                }
            }

            // Special Codes
            if (code == clearCode)
            {
                // Reset LZW
                codeTableIndex = endOfInformationCode + 1;
                codeSize = minCodeSize + 1;
                previousCode = -1;
                continue;
            }

            if (code == endOfInformationCode)
                // End of Information
                break;

            if (previousCode == -1)
            {
                // Initial Code
                indexStream.AddRange(CodeTable[code]);
                previousCode = code;
                continue;
            }

            // Compare to Code Table
            if (code < codeTableIndex)
            {
                // Get New Code
                var currentCodeArray = CodeTable[code];
                var previousCodeArray = CodeTable[previousCode];
                var newCode = new ushort[previousCodeArray.Length + 1];
                previousCodeArray.CopyTo(newCode, 0);
                newCode[newCode.Length - 1] = currentCodeArray[0];

                // Add to Index Stream
                indexStream.AddRange(currentCodeArray);

                // Add to Code Table
                if (codeTableIndex < CodeTable.Length)
                    CodeTable[codeTableIndex] = newCode;
            }
            else
            {
                // Get New Code
                var previousCodeArray = CodeTable[previousCode];
                var newCode = new ushort[previousCodeArray.Length + 1];
                previousCodeArray.CopyTo(newCode, 0);
                newCode[newCode.Length - 1] = previousCodeArray[0];

                // Add to Index Stream
                indexStream.AddRange(newCode);

                // Add to Code Table
                if (codeTableIndex < CodeTable.Length)
                    CodeTable[codeTableIndex] = newCode;
            }

            // Increase Code Table Index
            codeTableIndex++;

            // Update Previous Code
            previousCode = code;

            // Increase Code Size
            if (codeTableIndex >= 1 << codeSize && codeSize < 12)
                codeSize++;
        }

        // Fill in the rest of the index stream
        while (indexStream.Count < expectedSize)
            indexStream.Add(0);

        // Free Memory
        for (var k = endOfInformationCode + 1; k < CodeTable.Length; k++)
            CodeTable[k] = [];

        return indexStream;
    }

    /// <summary>
    ///     Pre-renders all frames of the GIF. Requires the GIF to be loaded.
    /// </summary>
    public void RenderAllFrames()
    {
        RenderFrame(Frames.Count - 1);
    }

    /// <summary>
    ///     Renders a frame of the GIF. Requires the GIF to be loaded.
    ///     Due to how GIFs are compressed, this will result in all previous frames being rendered as well.
    /// </summary>
    /// <param name="frameIndex">Index of the frame to render to a Sprite</param>
    public void RenderFrame(int frameIndex)
    {
        if (!IsLoaded)
            throw new Exception("GIF is not loaded");
        if (frameIndex < 0 || frameIndex >= Frames.Count)
            throw new Exception($"Frame index {frameIndex} is out of range");

#if PROFILING
        using var _ = Profiler.Measure(
            "GIFFile.RenderFrame",
            $"{Name}_frame_{frameIndex}");
#endif

        // Create pixel buffer
        if (_pixelBuffer == null)
        {
            _pixelBuffer = new uint[Width * Height];

            // Fill pixel buffer with background color
            _pixelBuffer.AsSpan().Fill(_backgroundColor.Value);
        }

        // Render all frames up to the target frame
        for (var i = 0; i <= frameIndex; i++)
        {
            // Frame
            var frame = Frames[i];
            if (frame.IsRendered) // Skip rendered frames
                continue;
            if (frame.IndexStream == null)
                throw new Exception($"Frame {i} index stream is null");
            var graphicsControl = frame.GraphicsControl;

            // Create temp pixel buffer
            uint[]? tempBuffer = null;
            if (frame.DisposalMethod == FrameDisposalMethod.RestoreToPrevious)
            {
                tempBuffer = new uint[_pixelBuffer.Length];
                _pixelBuffer.CopyTo(tempBuffer, 0);
            }

            // Get frame data
            var colorTable = frame.HasLocalColorTable ? frame.LocalColorTable : null;
            colorTable ??= _globalColorTable;

            var x = frame.LeftPosition;
            var y = frame.TopPosition;
            var w = frame.Width;
            var h = frame.Height;

            // Cache Transparent Index
            var transparentIndex = graphicsControl?.TransparentColorFlag ?? false
                ? graphicsControl.TransparentColorIndex
                : -1;

            // Loop through pixels
            var streamIndex = 0;

            for (var newY = 0; newY < h; newY++)
            {
                var pixelIndex = (Height - 1 - (y + newY)) * Width + x;
                var end = streamIndex + w;
                for (; streamIndex < end; streamIndex++, pixelIndex++)
                {
                    var colorIndex = frame.IndexStream[streamIndex];
                    if (colorIndex != transparentIndex)
                        _pixelBuffer[pixelIndex] = colorTable[colorIndex].Value;
                }
            }

            // Free memory
            frame.IndexStream = null;

            // Create frame sprite
            GenerateSpriteFromBuffer(
                $"{Name}[{i}]",
                out var texture,
                out var sprite
            );
            frame.RenderedTexture = texture;
            frame.RenderedSprite = sprite;

            // Handle frame disposal
            switch (frame.DisposalMethod)
            {
                case FrameDisposalMethod.RestoreToPrevious:
                    if (tempBuffer == null)
                        throw new Exception("Temp buffer is null when restoring to previous frame");
                    tempBuffer.CopyTo(_pixelBuffer, 0);
                    break;
                case FrameDisposalMethod.RestoreToBackgroundColor:
                    for (var row = 0; row < h; row++)
                    {
                        var pixelIndex = (Height - 1 - (y + row)) * Width + x;
                        _pixelBuffer
                            .AsSpan(pixelIndex, w)
                            .Fill(_backgroundColor.Value);
                    }

                    break;
                case FrameDisposalMethod.NoDisposal:
                case FrameDisposalMethod.DoNotDispose:
                default:
                    // Do nothing
                    break;
            }
        }

        // If this is the last frame, free the pixel buffer
        if (frameIndex >= Frames.Count - 1)
            _pixelBuffer = null;
    }

    private void GenerateSpriteFromBuffer(
        string name,
        out Texture2D texture,
        out Sprite sprite)
    {
        var pixelArtMode = GameConfiguration.CurrentMap?.properties.pixelArtMode ?? false;
        texture = new Texture2D(
            Width,
            Height,
            TextureFormat.RGBA32,
            false)
        {
            name = $"{name}_tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };

        // Load Texture Data
        PushToOutputBuffer();
        texture.LoadRawTextureData(_outputBuffer!.BasePointer, Width * Height * 4);
        texture.Apply(false, true);

        GCHandler.Register(texture, _gcBehavior);

        sprite = Sprite.Create(
            texture,
            new Rect(0, 0, Width, Height),
            new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );
        sprite.name = $"{name}_sprite";
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;

        GCHandler.Register(sprite, _gcBehavior);
    }

    /// <summary>
    ///     Pushes the contents of _pixelBuffer to _outputBuffer.
    ///     Doing this manually would be extremely resource-intensive.
    ///     As an optimization, we use Buffer.MemoryCopy on the raw pointers instead.
    /// </summary>
    private unsafe void PushToOutputBuffer()
    {
        if (_pixelBuffer == null)
            throw new NullReferenceException("Pixel buffer is null");

        // Resize output buffer
        var minOutputBufferSize = Width * Height * 4;
        if (_outputBuffer == null || _outputBuffer.Length < minOutputBufferSize)
            _outputBuffer = new MemoryBlock(minOutputBufferSize);

        // Push pixels to output buffer
        fixed (uint* srcPtr = _pixelBuffer)
        {
            Buffer.MemoryCopy(
                srcPtr,
                (void*)_outputBuffer.BasePointer,
                _outputBuffer.Length,
                _pixelBuffer.Length * 4);
        }
    }

    /// <summary>
    ///     Represents data for graphics control
    /// </summary>
    public class GIFGraphicsControl
    {
        public float Delay { get; init; } // seconds
        public FrameDisposalMethod DisposalMethod { get; init; }
        public bool TransparentColorFlag { get; init; }
        public int TransparentColorIndex { get; init; }
    }

    /// <summary>
    ///     Represents the data needed to draw a single frame of a GIF.
    /// </summary>
    public class GIFFrame
    {
        // Graphic Control Extension
        public GIFGraphicsControl? GraphicsControl { get; init; }

        public float Delay => GraphicsControl?.Delay ?? 0;

        public FrameDisposalMethod DisposalMethod =>
            GraphicsControl?.DisposalMethod ?? FrameDisposalMethod.DoNotDispose;

        public bool IsRendered => IndexStream == null;

        // Image Descriptor
        public ColorData[]? LocalColorTable { get; init; }
        public bool HasLocalColorTable { get; init; }
        public bool InterlaceFlag { get; init; }
        public bool SortFlag { get; init; }

        public int LeftPosition { get; init; }
        public int TopPosition { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        public List<ushort>? IndexStream { get; set; }

        public Texture2D? RenderedTexture { get; set; }
        public Sprite? RenderedSprite { get; set; }
    }

    public readonly struct ColorData(byte r, byte g, byte b, byte a)
    {
        public static readonly ColorData Clear = new(0, 0, 0, 0);

        public readonly uint Value = r |
                                     ((uint)g << 8) |
                                     ((uint)b << 16) |
                                     ((uint)a << 24);
    }
}