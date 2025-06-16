using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;
using UnityEngine.Networking;
using ByteArray = Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>;

namespace LevelImposter.Shop;

/// <summary>
///     Handles async HTTP Requests within Unity
/// </summary>
public class HTTPHandler(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public static HTTPHandler? Instance;

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

    /// <summary>
    ///     Coroutine to handle HTTP Requests
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="onSuccessString">Callback on success as a continuous string</param>
    /// <param name="onSuccessBytes">Callback on success as a byte stream</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onError">Callback on error with error info</param>
    [HideFromIl2Cpp]
    private IEnumerator CoRequest(string url,
        Action<string>? onSuccessString,
        Action<ByteArray>? onSuccessBytes,
        Action<float>? onProgress,
        Action<string>? onError)
    {
        {
            // Handle Request
            LILogger.Info($"GET: {url}");
            var request = UnityWebRequest.Get(url); // Doesn't extend IDisposable

            // Allow 404/500 errors
            request.SendWebRequest();
            while (!request.isDone)
            {
                if (onProgress != null)
                    onProgress(request.downloadProgress);
                yield return null;
            }

            // Handle Response
            LILogger.Info($"RES: {request.responseCode}");
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                if (onError != null)
                    onError(request.error);
            }
            else if (onSuccessString != null)
            {
                onSuccessString(request.downloadHandler.text);
            }
            else if (onSuccessBytes != null)
            {
                onSuccessBytes(request.downloadHandler.GetUnstrippedData());
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
    ///     Sends an async HTTP Request
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="onSuccessString">Callback on success as a continuous string</param>
    /// <param name="onSuccessBytes">Callback on success as a byte stream</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onError">Callback on error with error info</param>
    [HideFromIl2Cpp]
    private void Request(string url,
        Action<string>? onSuccessString,
        Action<ByteArray>? onSuccessBytes,
        Action<float>? onProgress,
        Action<string>? onError)
    {
        StartCoroutine(CoRequest(url, onSuccessString, onSuccessBytes, onProgress, onError).WrapToIl2Cpp());
    }

    // Shorthand overloads
    [HideFromIl2Cpp]
    public void Request(string url, Action<string>? onSuccess, Action<string>? onError)
    {
        Request(url, onSuccess, null, null, onError);
    }

    [HideFromIl2Cpp]
    public void Request(string url, Action<ByteArray>? onSuccess, Action<string>? onError)
    {
        Request(url, null, onSuccess, null, onError);
    }

    [HideFromIl2Cpp]
    public void Download(string url, Action<float>? onProgress, Action<string>? onSuccess,
        Action<string>? onError)
    {
        Request(url, onSuccess, null, onProgress, onError);
    }

    [HideFromIl2Cpp]
    public void Download(string url, Action<float>? onProgress, Action<ByteArray>? onSuccess,
        Action<string>? onError)
    {
        Request(url, null, onSuccess, onProgress, onError);
    }
}