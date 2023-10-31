using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;

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
        private JsonSerializerOptions _jsonOptions = new();
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
        private IEnumerator CoGet<T>(string filePath, Action<T?>? onSuccess, Action<string>? onError) where T : LIMetadata, new()
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
                    // Open File Stream
                    JsonObject mapJsonObj = new();
                    FileChunkConverter.FilePath = filePath;
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        // Iterate through each element
                        using JsonDocument jsonDoc = JsonDocument.Parse(fileStream);
                        JsonElement jsonRoot = jsonDoc.RootElement;
                        bool isMetadata = typeof(T) == typeof(LIMetadata);
                        foreach (JsonProperty element in jsonRoot.EnumerateObject())
                        {
                            // Async Delay
                            yield return null;

                            // Skip fields labeled as "elements" or "properties"
                            if (isMetadata && (element.Name == "elements" || element.Name == "properties"))
                                continue;

                            // Deserialize
                            mapJsonObj[element.Name] = JsonSerializer.Deserialize<JsonNode>(element.Value.GetRawText());
                        }
                    }

                    // Deserialize Json Object
                    T? mapData = JsonSerializer.Deserialize<T>(mapJsonObj.ToString());

                    // Check if deserialization failed
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
        public void Get<T>(string filePath, Action<T?>? onSuccess, Action<string>? onError) where T : LIMetadata, new()
        {
            StartCoroutine(CoGet(filePath, onSuccess, onError).WrapToIl2Cpp());
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _jsonOptions.Converters.Add(new FileChunkConverter());
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