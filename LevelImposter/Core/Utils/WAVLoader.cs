using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    /// <summary>
    /// Converts byte[] of WAV files into AudioClips
    /// </summary>
    public class WAVLoader
    {
        static float bytesToFloat(byte firstByte, byte secondByte)
        {
            return ((short)((secondByte << 8) | firstByte)) / 32768.0F;
        }

        static int bytesToInt(byte[] bytes, int offset = 0)
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                value |= ((int)bytes[offset + i]) << (i * 8);
            }
            return value;
        }

        public static AudioClip Load(byte[] wav)
        {
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

            // Make Audio Clip
            AudioClip clip = AudioClip.Create(
                "testSound",
                sampleCount,
                channelCount,
                frequency,
                false,
                false
            );
            clip.SetData(pcmData, 0);

            return clip;
        }
    }
}
