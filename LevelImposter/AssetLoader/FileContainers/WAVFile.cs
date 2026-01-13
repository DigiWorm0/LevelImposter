using System;
using System.IO;
using System.Linq;
using LevelImposter.Shop;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Core;

/// <summary>
///     Represents a WAV file.
/// </summary>
public class WAVFile(string _name) : IDisposable
{
    private const string CLIP_NAME = "WAVFile";

    private short _channelCount;
    private AudioClip? _clip;
    private float[]? _data;
    private int _sampleRate;

    public bool IsLoaded { get; private set; }

    public void Dispose()
    {
        if (_clip != null)
            Object.Destroy(_clip);
        _clip = null;
        _data = null;
    }

    /// <summary>
    ///     Loads the WAV file from the given stream
    /// </summary>
    /// <param name="dataStream">Data stream to read from</param>
    public void Load(Stream dataStream)
    {
        using var reader = new BinaryReader(dataStream);

        IsLoaded = false;
        ReadHeader(reader);
        while (ReadBlock(reader))
        {
        }

        GenerateClip();
        IsLoaded = true;
    }

    /// <summary>
    ///     Verifies the WAV file header
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadHeader(BinaryReader reader)
    {
        // RIFF header
        var isWAV = new string(reader.ReadChars(4)) == "RIFF";
        if (!isWAV)
            throw new Exception("File is not a RIFF file");
        
        // Chunk size
        var chunkSize = reader.ReadInt32();
        if (chunkSize != reader.BaseStream.Length - 8)
            throw new Exception("Header chunk size is not equal to file size");

        // WAVE header
        var isWave = new string(reader.ReadChars(4)) == "WAVE";
        if (!isWave)
            throw new Exception("File is not a WAV file");
    }

    /// <summary>
    ///     Reads a block from the WAV file
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    /// <returns><c>true</c> if the block was read, <c>false</c> if the end of the file was reached</returns>
    private bool ReadBlock(BinaryReader reader)
    {
        var blockName = new string(reader.ReadChars(4));
        if (string.IsNullOrEmpty(blockName))
            return false;
        
        switch (blockName)
        {
            case "fmt ":
                ReadFormatBlock(reader);
                return true;
            case "data":
                ReadDataBlock(reader);
                return true;
            default:
                // Skip unknown block (INFO, etc.)
                var chunkSize = reader.ReadInt32();
                SkipBytes(reader, chunkSize);
                return true;
        }
    }

    /// <summary>
    ///     Reads the format block of the WAV file
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadFormatBlock(BinaryReader reader)
    {
        // Chunk size
        var chunkSize = reader.ReadInt32();
        if (chunkSize != 16)
            throw new Exception("Format block size is not 16");

        // Audio format
        int audioFormat = reader.ReadInt16();
        if (audioFormat != 1)
            throw new Exception("Audio format is not PCM");

        // Format
        _channelCount = reader.ReadInt16();
        _sampleRate = reader.ReadInt32();

        // Unused bytes
        SkipBytes(reader, chunkSize - 8);
    }
    
    /// <summary>
    /// Skips the specified number of bytes in the binary reader.
    /// If the underlying stream supports seeking, it adjusts the position directly.
    /// Otherwise, it reads and discards the bytes.
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    /// <param name="byteCount">The number of bytes to skip</param>
    private void SkipBytes(BinaryReader reader, int byteCount)
    {
        if (reader.BaseStream.CanSeek)
            reader.BaseStream.Position += byteCount;
        else
            reader.ReadBytes(byteCount);
    }

    /// <summary>
    ///     Reads the data block of the WAV file
    /// </summary>
    /// <param name="reader">The binary reader to read from</param>
    private void ReadDataBlock(BinaryReader reader)
    {
        // Chunk Size
        var chunkSize = reader.ReadInt32();

        // Read Floats
        _data = new float[chunkSize / 2];
        for (var i = 0; i < _data.Length; i++)
            _data[i] = reader.ReadInt16() / 32768f;
    }

    /// <summary>
    ///     Generates an AudioClip from the WAV file
    /// </summary>
    private void GenerateClip()
    {
        if (_data == null)
            throw new Exception("WAV data is not loaded");

        _clip = AudioClip.Create(_name, _data.Length, _channelCount, _sampleRate, false);
        _clip.SetData(_data, 0);
        _clip.hideFlags = HideFlags.HideAndDontSave;

        _data = null; // Free memory
    }

    /// <summary>
    ///     Gets the AudioClip from the WAV file
    /// </summary>
    /// <returns>The UnityEngine AudioClip</returns>
    public AudioClip GetClip()
    {
        if (!IsLoaded || _clip == null)
            throw new Exception("WAV file is not loaded");
        return _clip;
    }
}