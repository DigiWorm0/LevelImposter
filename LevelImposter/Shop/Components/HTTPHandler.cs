using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace LevelImposter.Shop;

/// <summary>
///     Handles async HTTP Requests within Unity
/// </summary>
public class HTTPHandler(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public static HTTPHandler? Instance;
    
    /// <summary>
    /// The size of the buffer to use when downloading files.
    /// </summary>
    private const int DOWNLOAD_BUFFER_SIZE = 8192;

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
    /// Downloads a file asynchronously from the given URL and saves it to the specified file path using dotnet HttpClient.
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onSuccess">Callback on success</param>
    [HideFromIl2Cpp]
    public void DownloadFile(
        string url,
        string filePath,
        Action<float>? onProgress,
        Action? onSuccess)
    {
        var coroutine = CoDownloadFile(
            url,
            filePath,
            onProgress,
            onSuccess);
        StartCoroutine(coroutine.WrapToIl2Cpp());
    }
    
    /// <summary>
    ///     Background coroutine to handle file downloads
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onSuccess">Callback on success</param>
    [HideFromIl2Cpp]
    private static IEnumerator CoDownloadFile(
        string url,
        string filePath,
        Action<float>? onProgress,
        Action? onSuccess)
    {
        // Log start
        LILogger.Info($"DOWNLOAD: {url} >> {filePath}");
        
        // Start the task on a background thread
        var progress = 0f;
        using var task = DownloadFileTask(url, filePath, v => progress = v);
        
        // Wait for the task to complete
        while (!task.IsCompleted)
        {
            // Report progress
            onProgress?.Invoke(progress);
            yield return null;
        }
        
        // Check for errors
        if (task.IsFaulted)
            yield break;
        
        // Log completion
        LILogger.Info($"DONE: {filePath}");
        onSuccess?.Invoke();
    }

    /// <summary>
    ///     Background task to download a file using dotnet HttpClient.
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1. Warning: This is not called on the main Unity thread.</param>
    [HideFromIl2Cpp]
    private static async Task DownloadFileTask(
        string url,
        string filePath,
        Action<float>? onProgress)
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (request, cert, _, errors) =>
            {
                // HACK: Bypass SSL error for LevelImposter API on mobile
                // This is due to an issue where root certificates are inaccessible at runtime with HttpClient on some mobile platforms
                if (cert?.Subject == "CN=storage.googleapis.com" &&
                    request.RequestUri?.Host == "storage.googleapis.com" && 
                    LIConstants.IsMobile)
                    return true;
                
                if (cert?.Subject == "CN=levelimposter.net" &&
                    request.RequestUri?.Host == "api.levelimposter.net" &&
                    LIConstants.IsMobile)
                    return true;
                
                return errors == SslPolicyErrors.None;
            };
            
            using var httpClient = new HttpClient(handler);
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Make download buffer
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var totalBytesRead = 0L;
            var buffer = new byte[DOWNLOAD_BUFFER_SIZE];

            // Open file stream to a temporary file
            var tempFilePath = filePath + ".part";
            await using var fileStream = new FileStream(
                tempFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                DOWNLOAD_BUFFER_SIZE,
                useAsync: true);

            // Open response stream
            await using var contentStream = response.Content.ReadAsStreamAsync().Result;

            // Read from response stream
            int bytesRead;
            do
            {
                // Read chunk
                bytesRead = contentStream.ReadAsync(buffer, 0, buffer.Length).Result;

                // Write to file
                fileStream.WriteAsync(buffer, 0, bytesRead).Wait();
                totalBytesRead += bytesRead;

                // Report progress
                //      It's possible totalBytes is 0 if the server doesn't send a Content-Length header.
                //      In this case, we don't report progress.
                //      Ideally, LI API would always send Content-Length headers for files.
                if (totalBytes > 0)
                    onProgress?.Invoke((float)totalBytesRead / totalBytes);
            } while (bytesRead > 0);

            // Move temp file to final location
            if (File.Exists(filePath))
                File.Delete(filePath);
            fileStream.Close(); // <-- Ensure file is closed before moving
            File.Move(tempFilePath, filePath);
        }
        catch (HttpRequestException ex)
        {
            LILogger.Error($"HTTP Error downloading file from {url} to {filePath}:\n{ex}");
            if (ex.InnerException != null)
            {
                LILogger.Error($"Inner Exception: {ex.InnerException}");

                if (ex.InnerException.InnerException != null)
                {
                    LILogger.Error($"Inner Inner Exception: {ex.InnerException.InnerException}");
                }
            }
        }
        catch (Exception e)
        {
            LILogger.Error($"Error downloading file from {url} to {filePath}:\n{e}");
        }
    }
    
    /// <summary>
    ///     Sends an asynchronous request over HTTP(S) to the given URL using UnityWebRequest.
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="callback">Callback once request has completed</param>
    [HideFromIl2Cpp]
    public void Request(string url, Action<string>? callback)
    {
        StartCoroutine(CoRequest(url, callback).WrapToIl2Cpp());
    }
    
    /// <summary>
    ///     Background coroutine to handle HTTP Requests
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="onSuccessString">Callback on success as a continuous string</param>
    [HideFromIl2Cpp]
    private static IEnumerator CoRequest(
        string url,
        Action<string>? onSuccessString)
    {
        // Start the request
        LILogger.Info($"GET: {url}");
        var request = UnityWebRequest.Get(url);
        
        // Wait for response
        yield return request.SendWebRequest();
        LILogger.Info($"RES: {request.responseCode}");
        
        // Warn for 404/500 errors
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            // TODO: Handle errors properly
            //onError?.Invoke($"HTTP Error: {request.responseCode} - {request.error}");
            yield break;
        }
        
        // Success
        onSuccessString?.Invoke(request.downloadHandler.text);
    }
}