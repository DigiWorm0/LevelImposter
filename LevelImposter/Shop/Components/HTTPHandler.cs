using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Handles async HTTP Requests within Unity
    /// </summary>
    public class HTTPHandler : MonoBehaviour
    {
        public HTTPHandler(IntPtr intPtr) : base(intPtr)
        {
        }

        public static HTTPHandler? Instance = null;

        /// <summary>
        /// Coroutine to handle HTTP Requests
        /// </summary>
        /// <param name="url">URL to send request to</param>
        /// <param name="onSuccessString">Callback on success as a continuous string</param>
        /// <param name="onSuccessBytes">Callback on success as a byte stream</param>
        /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
        /// <param name="onError">Callback on error with error info</param>
        [HideFromIl2Cpp]
        private IEnumerator CoRequest(string url, 
            Action<string>? onSuccessString,
            Action<byte[]>? onSuccessBytes,
            Action<float>? onProgress,
            Action<string>? onError)
        {
            {
                // Handle Request
                LILogger.Info($"GET: {url}");
                UnityWebRequest? request = UnityWebRequest.Get(url); // Doesn't extend IDisposable
                request.SendWebRequest();
                while (!request.isDone)
                {
                    if (onProgress != null)
                        onProgress(request.downloadProgress);
                    yield return null;
                }

                // Handle Response
                LILogger.Info($"RES: {request.responseCode}");
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    LILogger.Error(request.error);
                    if (onError != null)
                        onError(request.error);
                }
                else if (onSuccessString != null)
                {
                    onSuccessString(request.downloadHandler.text);
                }
                else if (onSuccessBytes != null)
                {
                    onSuccessBytes(request.downloadHandler.data);
                }

                // Free memory (because BepInEx)
                request.Dispose();
                request = null;
                url = "";
                onSuccessString = null;
                onSuccessBytes = null;
                onProgress = null;
                onError = null;
            }
        }

        /// <summary>
        /// Sends an async HTTP Request
        /// </summary>
        /// <param name="url">URL to send request to</param>
        /// <param name="onSuccessString">Callback on success as a continuous string</param>
        /// <param name="onSuccessBytes">Callback on success as a byte stream</param>
        /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
        /// <param name="onError">Callback on error with error info</param>
        [HideFromIl2Cpp]
        private void Request(string url,
            Action<string>? onSuccessString,
            Action<byte[]>? onSuccessBytes,
            Action<float>? onProgress,
            Action<string>? onError)
        {
            StartCoroutine(CoRequest(url, onSuccessString, onSuccessBytes, onProgress, onError).WrapToIl2Cpp());
        }

        // Shorthand overloads
        [HideFromIl2Cpp] public void Request(string url, Action<string>? onSuccess, Action<string>? onError) => Request(url, onSuccess, null, null, onError);
        [HideFromIl2Cpp] public void Request(string url, Action<byte[]>? onSuccess, Action<string>? onError) => Request(url, null, onSuccess, null, onError);
        [HideFromIl2Cpp] public void Download(string url, Action<float>? onProgress, Action<string>? onSuccess, Action<string>? onError) => Request(url, onSuccess, null, onProgress, onError);
        [HideFromIl2Cpp] public void Download(string url, Action<float>? onProgress, Action<byte[]>? onSuccess, Action<string>? onError) => Request(url, null, onSuccess, onProgress, onError);

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
    }
}