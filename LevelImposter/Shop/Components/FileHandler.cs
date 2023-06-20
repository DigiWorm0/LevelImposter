using System;
using System.IO;
using System.Collections;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Text.Json;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Handles async File IO within Unity
    /// </summary>
    public class FileHandler : MonoBehaviour
    {
        public FileHandler(IntPtr intPtr) : base(intPtr)
        {
        }

        public static FileHandler? Instance = null;

        private const float MIN_FRAMERATE = 30.0f;
        private Stopwatch _loadTimer = new();
        private bool _shouldLoad
        {
            get { return _loadTimer.ElapsedMilliseconds <= (1000.0f / MIN_FRAMERATE); }
        }

        /// <summary>
        /// Coroutine to handle File IO
        /// </summary>
        /// <param name="filePath">Path to read file from</param>
        /// <param name="onSuccess">Callback on success with deserialized file data</param>
        /// <param name="onError">Callback on error with error info</param>
        /// <returns></returns>
        [HideFromIl2Cpp]
        private IEnumerator CoGet<T>(string filePath, Action<T?>? onSuccess, Action<string>? onError)
        {
            {
                // Wait for timing
                while (!_shouldLoad)
                    yield return null;

                // Check if file exists
                if (!File.Exists(filePath))
                {
                    if (onError != null)
                        onError($"Could not find {filePath} in filesystem");
                }
                else
                {
                    // Read/Deserialize file
                    using FileStream mapStream = File.OpenRead(filePath);
                    T? mapData = JsonSerializer.Deserialize<T>(mapStream);
                    if (mapData == null)
                    {
                        if (onError != null)
                            onError($"Failed to read {filePath} from filesystem");
                    }
                    else
                    {
                        if (onSuccess != null)
                            onSuccess(mapData);
                    }
                }

                // Free memory (BepInEx coroutines are buggy af, GC is not called)
                filePath = "";
                onSuccess = null;
                onError = null;
            }
        }

        /// <summary>
        /// Gets a file from the filesystem and deserializes it
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="filePath">File path to read from</param>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public void Get<T>(string filePath, Action<T?>? onSuccess, Action<string>? onError)
        {
            StartCoroutine(CoGet(filePath, onSuccess, onError).WrapToIl2Cpp());
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public void Update()
        {
            _loadTimer.Restart();
        }
    }
}