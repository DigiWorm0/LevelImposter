using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Represents a WAV file.
    /// </summary>
    public class WAVFile : IDisposable
    {
        private const string CLIP_NAME = "WAVFile";

        public bool IsLoaded { get; private set; }

        private short _channelCount = 0;
        private int _sampleRate = 0;
        private float[]? _data = null;
        private AudioClip? _clip = null;

        /// <summary>
        /// Loads a WAV file from the given base64 string and adds to map's GC list
        /// </summary>
        /// <param name="base64">Base64 string to load from</param>
        /// <returns>A Unity AudioClip</returns>
        public static AudioClip? Load(string? base64)
        {
            try
            {
                if (base64 == null)
                    throw new ArgumentNullException("Wave file base64 string cannot be null");
                if (!base64.StartsWith("data:audio/wav;base64,"))
                    throw new ArgumentException("Base64 string is not a WAV file");

                // Load File
                var wavFile = new WAVFile();
                var data = Convert.FromBase64String(base64.Substring(22));
                using (var stream = new MemoryStream(data))
                    wavFile.Load(stream);

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
        /// Loads the WAV file from the given stream
        /// </summary>
        /// <param name="dataStream">Data stream to read from</param>
        public void Load(Stream dataStream)
        {
            using (var reader = new BinaryReader(dataStream))
            {
                IsLoaded = false;
                ReadHeader(reader);
                while (ReadBlock(reader)) { }
                GenerateClip();
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Verifies the WAV file header
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        private void ReadHeader(BinaryReader reader)
        {
            // RIFF header
            bool isWAV = new string(reader.ReadChars(4)) == "RIFF";
            if (!isWAV)
                throw new Exception("File is not a RIFF file");

            // Chunk size
            int chunkSize = reader.ReadInt32();
            if (chunkSize != reader.BaseStream.Length - 8)
                throw new Exception("Header chunk size is not equal to file size");

            // WAVE header
            bool isWave = new string(reader.ReadChars(4)) == "WAVE";
            if (!isWave)
                throw new Exception("File is not a WAV file");
        }

        /// <summary>
        /// Reads a block from the WAV file
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
                    int chunkSize = reader.ReadInt32();
                    reader.ReadBytes(chunkSize);
                    return true;
            }
        }

        /// <summary>
        /// Reads the format block of the WAV file
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        private void ReadFormatBlock(BinaryReader reader)
        {
            // Chunk size
            int chunkSize = reader.ReadInt32();
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
            reader.ReadBytes(chunkSize - 8);
        }

        /// <summary>
        /// Reads the data block of the WAV file
        /// </summary>
        /// <param name="reader">The binary reader to read from</param>
        private void ReadDataBlock(BinaryReader reader)
        {
            // Chunk Size
            int chunkSize = reader.ReadInt32();

            // Read Floats
            _data = new float[chunkSize / 2];
            for (int i = 0; i < _data.Length; i++)
                _data[i] = reader.ReadInt16() / 32768f;
        }

        /// <summary>
        /// Generates an AudioClip from the WAV file
        /// </summary>
        private void GenerateClip()
        {
            if (_data == null)
                throw new Exception("WAV data is not loaded");

            _clip = AudioClip.Create(CLIP_NAME, _data.Length, _channelCount, _sampleRate, false);
            _clip.SetData(_data, 0);

            _data = null; // Free memory
        }

        /// <summary>
        /// Gets the AudioClip from the WAV file
        /// </summary>
        /// <returns>The UnityEngine AudioClip</returns>
        public AudioClip GetClip()
        {
            if (!IsLoaded || _clip == null)
                throw new Exception("WAV file is not loaded");
            return _clip;
        }

        public void Dispose()
        {
            if (_clip != null)
                UnityEngine.Object.Destroy(_clip);
            _clip = null;
            _data = null;
        }
    }
}