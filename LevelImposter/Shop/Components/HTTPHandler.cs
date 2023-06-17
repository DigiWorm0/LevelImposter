using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using LevelImposter.Core;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Text.Json;
using Il2CppInterop.Runtime.Attributes;

namespace LevelImposter.Shop
{
    /// <summary>
    /// Handles HTTP Requests within Unity
    /// </summary>
    public class HTTPHandler : MonoBehaviour
    {
        public HTTPHandler(IntPtr intPtr) : base(intPtr)
        {
        }

        public static HTTPHandler? Instance = null;

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

                // Free memory (BepInEx coroutines are buggy af, GC is not called)
                request.Dispose();
                request = null;
                url = "";
                onSuccessString = null;
                onSuccessBytes = null;
                onProgress = null;
                onError = null;
            }
        }

        private void Request(string url,
            Action<string>? onSuccessString,
            Action<byte[]>? onSuccessBytes,
            Action<float>? onProgress,
            Action<string>? onError)
        {
            StartCoroutine(CoRequest(url, onSuccessString, onSuccessBytes, onProgress, onError).WrapToIl2Cpp());
        }

        public void Request(string url, Action<string>? onSuccess, Action<string>? onError) => Request(url, onSuccess, null, null, onError);
        public void Request(string url, Action<byte[]>? onSuccess, Action<string>? onError) => Request(url, null, onSuccess, null, onError);
        public void Download(string url, Action<float>? onProgress, Action<string>? onSuccess, Action<string>? onError) => Request(url, onSuccess, null, onProgress, onError);
        public void Download(string url, Action<float>? onProgress, Action<byte[]>? onSuccess, Action<string>? onError) => Request(url, null, onSuccess, onProgress, onError);

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