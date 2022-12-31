using System;
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

        public static WAVLoader Instance;

        public void Awake()
        {
            Instance = this;
        }

        [HideFromIl2Cpp]
        public void LoadWAV(LIElement element, LISound sound, Action<AudioClip> onLoad)
        {
            LILogger.Info($"Loading sound for {element}");
            LoadWAV(sound.data ?? "", (AudioClip audioClip) =>
            {
                LILogger.Info($"Done loading sound for {element}");
                onLoad(audioClip);
            });
        }

        [HideFromIl2Cpp]
        public void LoadWAV(string b64, Action<AudioClip> onLoad)
        {
            if (LIShipStatus.Instance == null)
            {
                LILogger.Error("Cannot load audio, LIShipStatus.Instance is null");
                return;
            }
            StartCoroutine(CoLoadAudio(b64, onLoad).WrapToIl2Cpp());
        }

        [HideFromIl2Cpp]
        private IEnumerator CoLoadAudio(string b64, Action<AudioClip> onLoad)
        {
            Task<AudioMetadata> task = Task.Run(() => { return ProcessWAV(b64); });
            while (!task.IsCompleted)
                yield return null;
            AudioMetadata audioData = task.Result;
            AudioClip audioClip = LoadWAV(audioData);
            onLoad.Invoke(audioClip);
        }

        [HideFromIl2Cpp]
        private AudioMetadata ProcessWAV(string b64Audio)
        {
            // Get Image Bytes
            byte[] wav = MapUtils.ParseBase64(b64Audio);

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

        [HideFromIl2Cpp]
        private AudioClip LoadWAV(AudioMetadata audioData)
        {
            AudioClip clip = AudioClip.Create(
                "LI Audio",
                audioData.sampleCount,
                audioData.channelCount,
                audioData.frequency,
                false,
                false
            );
            clip.SetData(audioData.pcmData, 0);

            return clip;
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
