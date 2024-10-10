using System;
using System.IO;
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

    public static AudioClip? LoadSound(LISound? soundData)
    {
        // Get Sound Data
        if (soundData == null)
            return null;

        // Get Asset DB
        var mapAssetDB = MapLoader.CurrentMap?.mapAssetDB;
        if (mapAssetDB == null)
            return null;

        // Get Sound Stream
        var soundDBElem = mapAssetDB.Get(soundData.dataID);
        if (soundDBElem == null)
            return null;

        // Get Sound Data
        using var stream = soundDBElem.OpenStream();
        return LoadStream(stream);
    }

    /// <summary>
    ///     Loads a WAV file from the given base64 string and adds to map's GC list
    /// </summary>
    /// <param name="dataStream">Data stream to load from</param>
    /// <returns>A Unity AudioClip</returns>
    public static AudioClip? LoadStream(Stream dataStream)
    {
        try
        {
            // Load File
            var wavFile = new WAVFile(CLIP_NAME);
            wavFile.Load(dataStream);

            // Add to GC list
            GCHandler.Register(wavFile);

            // Return clip
            return wavFile.GetClip();
        }
        catch (Exception e)
        {
            LILogger.Warn(e);
            return null;
        }
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
        if (reader.BaseStream.Position == reader.BaseStream.Length)
            return false;

        var blockName = new string(reader.ReadChars(4));
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
                reader.BaseStream.Position += chunkSize;
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
        reader.BaseStream.Position += chunkSize - 8;
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