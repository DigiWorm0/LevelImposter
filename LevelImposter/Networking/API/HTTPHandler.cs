using System;
using System.Collections;
using System.Text.Json;
using LevelImposter.Core;
using Reactor.Utilities;
using UnityEngine.Networking;
using FetchResult = LevelImposter.Networking.API.HTTPHandler.HTTPResult<string>;

namespace LevelImposter.Networking.API;

/// <summary>
///     Handles async HTTP Requests within Unity
/// </summary>
public static class HTTPHandler
{
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

        try
        {
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
    ///     Sends an asynchronous request over HTTP(S) to the given URL and parses the JSON response into the given type.
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
    ///     Represents the result of an HTTP request.
    /// </summary>
    /// <typeparam name="T">Type of the data returned by the request.</typeparam>
    public struct HTTPResult<T>
    {
        /// <summary>
        ///     Data returned by the request.
        /// </summary>
        public T? Data;

        /// <summary>
        ///     Indicates whether the request was successful.
        /// </summary>
        public string? ErrorText;
    }
}