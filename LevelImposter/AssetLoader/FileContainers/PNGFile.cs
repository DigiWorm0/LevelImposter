using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using LevelImposter.Builders.Util;
using LevelImposter.Core.GarbageCollection;
using LevelImposter.FileIO.Streams;
using UnityEngine;

namespace LevelImposter.AssetLoader.FileContainers;

/// <summary>
///     Represents a PNG file.
///     Used to build and render a PNG into a Unity Texture2D.
///     Based on W3C's PNG specification 1.0: https://www.w3.org/TR/png/
/// </summary>
/// <param name="name"></param>
public class PNGFile(string name, GCBehavior? gcBehavior = null)
{
    public enum PNGColorType : byte
    {
        Greyscale = 0,
        TrueColor = 2,
        IndexedColor = 3,
        GreyscaleWithAlpha = 4,
        TrueColorWithAlpha = 6
    }

    private static readonly uint[] CrcLookupTable = CreateCrcTable();

    private readonly Dictionary<PNGColorType, byte[]> _allowedBitDepths = new()
    {
        { PNGColorType.Greyscale, [1, 2, 4, 8, 16] },
        { PNGColorType.TrueColor, [8, 16] },
        { PNGColorType.IndexedColor, [1, 2, 4, 8] },
        { PNGColorType.GreyscaleWithAlpha, [8, 16] },
        { PNGColorType.TrueColorWithAlpha, [8, 16] }
    };

    private readonly MemoryStream _dataStream = new();
    private ColorData[]? _paletteData;
    private uint[]? _pixelBuffer;

    public Texture2D? OutputTexture { get; private set; }

    public bool IsLoaded { get; private set; }

    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public byte BitDepth { get; private set; }
    public PNGColorType ColorType { get; private set; }
    public byte CompressionMethod { get; private set; }
    public byte FilterMethod { get; private set; }
    public byte InterlaceMethod { get; private set; }

    /// <summary>
    ///     Checks if the given data is a PNG file.
    /// </summary>
    /// <param name="data">MemoryBlock of raw PNG data</param>
    /// <returns>True if the data is a PNG file. False otherwise</returns>
    public static bool IsPNG(byte[] data)
    {
        return data[0] == 137
               && data[1] == 'P'
               && data[2] == 'N'
               && data[3] == 'G'
               && data[4] == 13
               && data[5] == 10
               && data[6] == 26
               && data[7] == 10;
    }

    /// <summary>
    ///     Loads all the PNG data from the given data stream.
    ///     Can be done on any thread.
    /// </summary>
    /// <param name="dataStream">Stream containing the PNG data</param>
    public void Load(Stream dataStream)
    {
        if (IsLoaded)
            throw new InvalidOperationException("Already loaded");

        using var reader = new BinaryReader(dataStream);
        reader.ReadBytes(8); // <-- Skip signature bytes
        while (ReadChunk(reader))
        {
        }

        DeflateDataStream();
        IsLoaded = true;
    }

    /// <summary>
    ///     Renders the PNG data into a Unity Texture2D and Sprite.
    ///     Image data must be loaded beforehand using Load().
    ///     Must be done on the main Unity thread.
    /// </summary>
    public void Render()
    {
        if (!IsLoaded)
            throw new InvalidOperationException("Not loaded");
        GenerateTextureFromBuffer(out var texture);
        OutputTexture = texture;
    }

    private int GetChannelCount()
    {
        return ColorType switch
        {
            PNGColorType.Greyscale => 1,
            PNGColorType.TrueColor => 3,
            PNGColorType.IndexedColor => 1,
            PNGColorType.GreyscaleWithAlpha => 2,
            PNGColorType.TrueColorWithAlpha => 4,
            _ => throw new InvalidDataException("Unsupported color type.")
        };
    }

    private void DeflateDataStream()
    {
        _dataStream.Position = 2; // <-- Skip the zlib header
        using var deflateStream = new DeflateStream(
            _dataStream,
            CompressionMode.Decompress);

        // TODO: Support non-8 bit depths
        if (BitDepth != 8)
            throw new NotImplementedException("Only 8-bit PNGs are supported at this time.");

        // Cache common values
        var width = Width;
        var height = Height;
        var channelCount = GetChannelCount();
        var bytesPerScanline = checked(width * channelCount);

        // Initialize pixel buffer
        _pixelBuffer ??= new uint[checked(width * height)];

        // Iterate scanlines
        var scanlineBuffer = new byte[bytesPerScanline];
        var prevScanlineBuffer = new byte[bytesPerScanline];

        for (uint y = 0, offset = width * (height - 1); // <-- Reverses the scanlines
             y < height;
             y++, offset -= width)
        {
            var scanlineFilter = deflateStream.ReadByte();
            if (scanlineFilter < 0)
                throw new InvalidDataException("Unexpected end of PNG data stream");

            // Read line into buffer
            deflateStream.ReadExactly(scanlineBuffer, 0, scanlineBuffer.Length);

            // Apply filter
            ApplyScanlineFilter(
                scanlineFilter,
                scanlineBuffer,
                prevScanlineBuffer,
                channelCount);

            // Push to Pixel Buffer
            WriteScanlinePixels(
                offset,
                scanlineBuffer,
                width);

            // Swap prev/curr buffers
            // (This is faster than cloning the entire buffer's contents every iteration)
            (prevScanlineBuffer, scanlineBuffer) = (scanlineBuffer, prevScanlineBuffer);
        }
    }

    private void WriteScanlinePixels(
        uint offset,
        ReadOnlySpan<byte> source,
        uint width)
    {
        var destination = _pixelBuffer.AsSpan((int)offset, (int)Width);

        switch (ColorType)
        {
            case PNGColorType.Greyscale:
                for (int x = 0, i = 0; x < width; x++, i++)
                {
                    var value = source[i];
                    destination[x] = ColorData.RgbaToColor(
                        value,
                        value,
                        value,
                        255);
                }

                break;
            case PNGColorType.GreyscaleWithAlpha:
                for (int x = 0, i = 0; x < width; x++, i += 2)
                {
                    var value = source[i];
                    destination[x] = ColorData.RgbaToColor(
                        value,
                        value,
                        value,
                        source[i + 1]);
                }

                break;
            case PNGColorType.TrueColor:
                for (int x = 0, i = 0; x < width; x++, i += 3)
                    destination[x] = ColorData.RgbaToColor(
                        source[i],
                        source[i + 1],
                        source[i + 2],
                        255);
                break;
            case PNGColorType.TrueColorWithAlpha:
                for (int x = 0, i = 0; x < width; x++, i += 4)
                    destination[x] = ColorData.RgbaToColor(
                        source[i],
                        source[i + 1],
                        source[i + 2],
                        source[i + 3]);
                break;

            case PNGColorType.IndexedColor:
                throw new NotImplementedException();

            default:
                throw new Exception($"Unsupported color type: {ColorType}");
        }
    }

    private void ApplyScanlineFilter(
        int scanlineFilter,
        Span<byte> current,
        ReadOnlySpan<byte> previous,
        int bytesPerPixel)
    {
        switch (scanlineFilter)
        {
            case 0: // None
                break;
            case 1: // Sub
                for (var i = bytesPerPixel; i < current.Length; i++)
                    current[i] += current[i - bytesPerPixel];
                break;
            case 2: // Up
                for (var i = 0; i < current.Length; i++)
                    current[i] += previous[i];
                break;
            case 3: // Average

                // Apply to 1st N pixels
                // (Avoids branching in the loop by handling the first N pixels separately)
                for (var i = 0; i < bytesPerPixel; i++)
                    current[i] += (byte)(previous[i] >> 1);

                // Apply to the rest
                for (var i = bytesPerPixel; i < current.Length; i++)
                {
                    var a = current[i - bytesPerPixel];
                    var b = previous[i];

                    current[i] += (byte)((a + b) >> 1);
                }

                break;
            case 4: // Paeth

                // Apply to 1st N pixels
                // (Avoids branching in the loop by handling the first N pixels separately)
                for (var i = 0; i < bytesPerPixel; i++)
                    current[i] += previous[i];

                // Apply to the rest
                for (var i = bytesPerPixel; i < current.Length; i++)
                {
                    var a = current[i - bytesPerPixel];
                    var b = previous[i];
                    var c = previous[i - bytesPerPixel];

                    var p = a + b - c;
                    var pa = p >= a ? p - a : a - p;
                    var pb = p >= b ? p - b : b - p;
                    var pc = p >= c ? p - c : c - p;

                    if (pa <= pb && pa <= pc)
                        current[i] += a;
                    else if (pb <= pc)
                        current[i] += b;
                    else
                        current[i] += c;
                }

                break;
            default:
                throw new Exception($"Unsupported scanline filter: {scanlineFilter}");
        }
    }

    private bool ReadChunk(BinaryReader reader)
    {
        var length = reader.ReadUInt32_BigEndian();
        var typeBytes = reader.ReadBytes(4);
        var dataBytes = reader.ReadBytes((int)length);
        var crc = reader.ReadUInt32_BigEndian();

        var expectedCRC = CalculateCRC(typeBytes, dataBytes);
        if (expectedCRC != crc)
            throw new Exception($"CRC mismatch for chunk. Expected {expectedCRC:X8}, but got {crc:X8}.");

        var chunkType = Encoding.ASCII.GetString(typeBytes);
        switch (chunkType)
        {
            case "IHDR":
                ReadHeaderChunk(dataBytes);
                break;
            case "PLTE":
                ReadPaletteChunk(dataBytes);
                break;
            case "IDAT":
                ReadDataChunk(dataBytes);
                break;
            case "IEND":
                // End of file
                return false;
            default:
                var isCriticalChunk = chunkType[0] >= 'A' &&
                                      chunkType[0] <= 'Z'; // <-- Capital letter
                if (isCriticalChunk)
                    Debug.LogWarning(
                        $"Unknown critical chunk type '{chunkType}'. Skipping {dataBytes.Length} bytes.");

                // Ignore non-critical chunks
                break;
        }

        return true;
    }

    private void ReadHeaderChunk(byte[] data)
    {
        using var dataStream = new MemoryStream(data);
        using var reader = new BinaryReader(dataStream);

        Width = reader.ReadUInt32_BigEndian();
        Height = reader.ReadUInt32_BigEndian();
        if (Width == 0 || Height == 0)
            throw new Exception("Invalid PNG dimensions. Width and Height must be greater than 0.");

        BitDepth = reader.ReadByte();
        ColorType = (PNGColorType)reader.ReadByte();
        CompressionMethod = reader.ReadByte();
        FilterMethod = reader.ReadByte();
        InterlaceMethod = reader.ReadByte();

        // Verify header values are within range of the PNG specification
        if (!Enum.IsDefined(typeof(PNGColorType), ColorType))
            throw new Exception($"Unsupported PNG color type: {ColorType}");

        var allowedBitDepths = _allowedBitDepths[ColorType];
        if (!allowedBitDepths.Contains(BitDepth))
            throw new Exception($"Unsupported PNG bit depth: {BitDepth}");

        if (CompressionMethod != 0)
            throw new Exception($"Unsupported PNG compression method: ${CompressionMethod}");

        if (FilterMethod != 0)
            throw new Exception($"Unsupported PNG filter method: ${FilterMethod}");

        if (InterlaceMethod != 0 &&
            InterlaceMethod != 1)
            throw new Exception($"Unsupported PNG interlace method: ${InterlaceMethod}");
    }

    private void ReadPaletteChunk(byte[] data)
    {
        using var dataStream = new MemoryStream(data);
        using var reader = new BinaryReader(dataStream);

        if (data.Length % 3 != 0)
            throw new Exception("Invalid PLTE chunk length. Must be a multiple of 3.");

        _paletteData = new ColorData[data.Length / 3];
        for (var i = 0; i < _paletteData.Length; i++)
        {
            var r = reader.ReadByte();
            var g = reader.ReadByte();
            var b = reader.ReadByte();
            _paletteData[i] = new ColorData(r, g, b, 255);
        }
    }

    private void ReadDataChunk(byte[] data)
    {
        _dataStream.Write(data, 0, data.Length);
    }

    private static uint CalculateCRC(
        ReadOnlySpan<byte> chunkType,
        ReadOnlySpan<byte> chunkData)
    {
        if (chunkType.Length != 4)
            throw new ArgumentException("PNG chunk type must be exactly 4 bytes.", nameof(chunkType));

        var crc = 0xFFFFFFFFu;

        foreach (var b in chunkType)
            crc = CrcLookupTable[(crc ^ b) & 0xFF] ^ (crc >> 8);

        foreach (var b in chunkData)
            crc = CrcLookupTable[(crc ^ b) & 0xFF] ^ (crc >> 8);

        return crc ^ 0xFFFFFFFFu;
    }

    private static uint[] CreateCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0
                    ? (c >> 1) ^ 0xEDB88320u
                    : c >> 1;

            table[n] = c;
        }

        return table;
    }

    private unsafe void GenerateTextureFromBuffer(out Texture2D texture)
    {
        // var pixelArtMode = GameConfiguration.CurrentMap?.properties.pixelArtMode ?? false;
        var pixelArtMode = true;
        texture = new Texture2D(
            (int)Width,
            (int)Height,
            TextureFormat.RGBA32,
            false)
        {
            name = $"{name}_tex",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = pixelArtMode ? FilterMode.Point : FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave,
            requestedMipmapLevel = 0
        };
        GCHandler.Register(texture, gcBehavior);

        // Load Texture Data
        fixed (uint* pArray = _pixelBuffer)
        {
            texture.LoadRawTextureData((IntPtr)pArray, (int)(Width * Height * 4));
            texture.Apply(false, true);
        }
    }

    private readonly struct ColorData(byte r, byte g, byte b, byte a)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RgbaToColor(byte r, byte g, byte b, byte a)
        {
            return r |
                   ((uint)g << 8) |
                   ((uint)b << 16) |
                   ((uint)a << 24);
        }

        public readonly uint Value = RgbaToColor(r, g, b, a);
    }
}