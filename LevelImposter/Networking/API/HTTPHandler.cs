using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Text.Json;
using System.Threading.Tasks;
using LevelImposter.Core;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.Networking;

using DownloadResult = LevelImposter.Networking.API.HTTPHandler.HTTPResult<LevelImposter.Core.FileStore>;
using FetchResult = LevelImposter.Networking.API.HTTPHandler.HTTPResult<string>;

namespace LevelImposter.Networking.API;

/// <summary>
///     Handles async HTTP Requests within Unity
/// </summary>
public static class HTTPHandler
{
    /// <summary>
    /// The size of the buffer to use when downloading files.
    /// </summary>
    private const int DOWNLOAD_BUFFER_SIZE = 8192;
    
    /// <summary>
    /// Downloads a file asynchronously from the given URL and saves it to the specified file path using dotnet HttpClient.
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onComplete">Callback on completion (or error)</param>
    public static void DownloadFile(
        string url,
        string filePath,
        Action<float>? onProgress,
        Action<DownloadResult>? onComplete)
    {
        Coroutines.Start(CoDownloadFile(url, filePath, onProgress, onComplete));
    }
    
    /// <summary>
    ///     Background coroutine to handle file downloads
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onComplete">Callback on completion (or error)</param>
    private static IEnumerator CoDownloadFile(
        string url,
        string filePath,
        Action<float>? onProgress,
        Action<DownloadResult>? onComplete)
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
        onComplete?.Invoke(task.Result);
    }

    /// <summary>
    ///     Background task to download a file using dotnet HttpClient.
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1. Warning: This is not called on the main Unity thread.</param>
    private static async Task<DownloadResult> DownloadFileTask(
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
                bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);

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

            // Return success
            return new DownloadResult
            {
                Data = new FileStore(filePath),
                ErrorText = null
            };
        }
        catch (Exception e)
        {
            LILogger.Error($"Error downloading file from {url} to {filePath}:\n{e}");

            // Return error
            return new DownloadResult
            {
                Data = null,
                ErrorText = e.Message
            };
        }
    }
    
    /// <summary>
    ///     Sends an asynchronous request over HTTP(S) to the given URL using UnityWebRequest.
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="callback">Callback once request has completed</param>
    public static void RequestText(string url, Action<FetchResult>? callback)
    {
        Coroutines.Start(CoRequestText(url, callback));
    }
    
    /// <summary>
    ///     Background coroutine to handle HTTP Requests
    /// </summary>
    /// <param name="url">URL to send request to</param>
    /// <param name="callback">Callback once request has completed</param>
    private static IEnumerator CoRequestText(
        string url,
        Action<FetchResult>? callback)
    {
        // Start the request
        LILogger.Info($"GET: {url}");
        var request = UnityWebRequest.Get(url);
        
        // Wait for response
        yield return request.SendWebRequest();
        LILogger.Info($"RES: {request.responseCode}");
        
        try {
            // Throw error on failure
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
                throw new Exception($"HTTP Error {request.responseCode}: {request.error}");

            // Return response text on success
            callback?.Invoke(new FetchResult
            {
                Data = request.downloadHandler.text,
                ErrorText = null
            });
        }
        catch (Exception e)
        {
            LILogger.Error($"HTTP Request Error: {e}");

            // Return error
            callback?.Invoke(new FetchResult
            {
                Data = null,
                ErrorText = e.Message
            });
        }
    }

    /// <summary>
    ///   Sends an asynchronous request over HTTP(S) to the given URL and parses the JSON response into the given type.
    /// </summary>
    /// <typeparam name="T">Type to parse the JSON response into</typeparam>
    /// <param name="url">URL to send request to</param>
    /// <param name="callback">Callback once request has completed</param>
    public static void RequestJSON<T>(string url, Action<HTTPResult<T>>? callback)
    {
        RequestText(url, result => ParseJSONResponse(result, callback));
    }
    private static void ParseJSONResponse<T>(HTTPResult<string> result, Action<HTTPResult<T>>? callback)
    {
        // Handle HTTP errors
        if (result.ErrorText != null)
        {
            callback?.Invoke(new HTTPResult<T>
            {
                Data = default,
                ErrorText = result.ErrorText
            });
            return;
        }

        try
        {
            // Attempt to deserialize JSON
            var data = JsonSerializer.Deserialize<T>(result.Data ?? "");
            callback?.Invoke(new HTTPResult<T>
            {
                Data = data,
                ErrorText = null
            });
        }
        catch (Exception e)
        {
            // Handle deserialization errors
            LILogger.Error($"JSON Deserialization Error: {e}");
            callback?.Invoke(new HTTPResult<T>
            {
                Data = default,
                ErrorText = "JSON Deserialization Error: " + e.Message
            });
        }
    }

    /// <summary>
    /// Represents the result of an HTTP request.
    /// </summary>
    /// <typeparam name="T">Type of the data returned by the request.</typeparam>
    public struct HTTPResult<T>
    {
        /// <summary>
        /// Data returned by the request.
        /// </summary>
        public T? Data;

        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public string? ErrorText;
    }
}