using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using System.Collections;

namespace LevelImposter.Core
{
    /// <summary>
    /// Converts byte[] of WAV files into AudioClips
    /// </summary>
    public class WAVLoader : MonoBehaviour
    {
        public WAVLoader(IntPtr intPtr) : base(intPtr)
        {
        }

        public const float MIN_FRAMERATE = 15.0f;

        public static WAVLoader? Instance;

        private Stopwatch? _loadTimer = new();
        private Stack<AudioClip>? _mapClips = new();
        private int _loadCount = 0;
        private bool _shouldLoad
        {
            get { return _loadTimer?.ElapsedMilliseconds <= (1000.0f / MIN_FRAMERATE); }
        }

        public int LoadCount => _loadCount;

        /// <summary>
        /// Loads WAV data of an LISound
        /// </summary>
        /// <param name="element">LIElement of orgin</param>
        /// <param name="sound">Sound data to load</param>
        /// <param name="onLoad">Callback on load</param>
        [HideFromIl2Cpp]
        public void LoadWAV(LIElement element, LISound sound, Action<AudioClip> onLoad)
        {
            LILogger.Info($"Loading sound for {element}");
            LoadWAV(sound.data ?? "", (audioClip) =>
            {
                if (audioClip == null)
                {
                    LILogger.Warn($"Error loading sound for {element}");
                    return;
                }

                LILogger.Info($"Done loading sound for {element}");
                onLoad(audioClip);
            });
        }

        /// <summary>
        /// Loads Audio data from base64
        /// </summary>
        /// <param name="b64">Base64 audio data</param>
        /// <param name="onLoad">Callback on load</param>
        [HideFromIl2Cpp]
        public void LoadWAV(string b64, Action<AudioClip?> onLoad)
        {
            if (LIShipStatus.Instance == null)
            {
                LILogger.Error("Cannot load audio, LIShipStatus.Instance is null");
                return;
            }
            StartCoroutine(CoLoadAudio(b64, onLoad).WrapToIl2Cpp());
        }

        /// <summary>
        /// Coroutine to load audio data from Base64
        /// </summary>
        /// <param name="b64">Base64 audio data</param>
        /// <param name="onLoad">Callback on load</param>
        [HideFromIl2Cpp]
        private IEnumerator CoLoadAudio(string b64, Action<AudioClip?> onLoad)
        {
            _loadCount++;
            while (!_shouldLoad)
                yield return null;
            AudioMetadata? audioData = ProcessWAV(b64);
            while (!_shouldLoad)
                yield return null;
            AudioClip audioClip = LoadWAV(audioData);
            if (onLoad != null)
                onLoad.Invoke(audioClip);
            onLoad = null;
            _loadCount--;
        }

        /// <summary>
        /// Processes audio data from Base64 to raw PCM
        /// </summary>
        /// <param name="b64Audio">Base64 audio data</param>
        /// <returns>Audio Metadata</returns>
        [HideFromIl2Cpp]
        private AudioMetadata? ProcessWAV(string b64Audio)
        {
            try
            {
                // Get Image Bytes
                MemoryStream dataStream = MapUtils.ParseBase64(b64Audio);
                byte[] wav = new byte[dataStream.Length];
                dataStream.Read(wav, 0, wav.Length);
                dataStream.Dispose();

                // Metadata
                int channelCount = wav[22];
                int frequency = bytesToInt(wav, 24);

                // Find Data Chunk
                int pos = 12;
                while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
                {
                    pos += 4;
                    int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                    pos += 4 + chunkSize;
                }
                pos += 8;

                // Get Sample Count
                int sampleCount = (wav.Length - pos) / (2 * channelCount);

                // Load Channel Data
                float[] pcmData = new float[sampleCount * channelCount];
                int i = 0;
                while (pos < wav.Length)
                {
                    pcmData[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                    if (channelCount == 2)
                    {
                        pcmData[i + 1] = bytesToFloat(wav[pos], wav[pos + 1]);
                        pos += 2;
                    }
                    i += channelCount;
                }


                // Return Metadata
                return new AudioMetadata()
                {
                    sampleCount = sampleCount,
                    channelCount = channelCount,
                    frequency = frequency,
                    pcmData = pcmData
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HideFromIl2Cpp]
        private float bytesToFloat(byte firstByte, byte secondByte)
        {
            return ((short)((secondByte << 8) | firstByte)) / 32768.0F;
        }

        [HideFromIl2Cpp]
        private int bytesToInt(byte[] bytes, int offset = 0)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= ((int)bytes[offset + i]) << (i * 8);
            }
            return value;
        }

        /// <summary>
        /// Loads AudioClip from AudioMetadata
        /// </summary>
        /// <param name="audioData">Audio Metadata from WAV file</param>
        /// <returns>AudioClip representing audio data</returns>
        [HideFromIl2Cpp]
        private AudioClip? LoadWAV(AudioMetadata? audioData)
        {
            if (audioData == null)
                return null;
            AudioClip clip = AudioClip.Create(
                "LI Audio",
                audioData.sampleCount,
                audioData.channelCount,
                audioData.frequency,
                false,
                false
            );
            clip.SetData(audioData.pcmData, 0);
            _mapClips?.Push(clip);

            return clip;
        }

        public void Awake()
        {
            Instance = this;
        }
        public void Update()
        {
            if (_loadCount > 0)
                _loadTimer?.Restart();
        }
        public void OnDestroy()
        {
            LILogger.Info("Destroying " + _mapClips?.Count + " map sounds");
            while (_mapClips?.Count > 0)
                Destroy(_mapClips.Pop());
            _mapClips = null;
            _loadTimer = null;
        }

        private class AudioMetadata
        {
            public int sampleCount = 0;
            public int channelCount = 0;
            public int frequency = 0;
            public float[] pcmData = Array.Empty<float>();
        }
    }
}
