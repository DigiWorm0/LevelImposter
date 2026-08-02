using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using LevelImposter.AssetLoader.Queue;
using LevelImposter.Core;
using LevelImposter.FileIO.DataStores;
using Reactor.Utilities;

namespace LevelImposter.Networking.API;

public class FileDownloader
{
    /// <summary>
    ///     The size of the buffer to use when downloading files.
    /// </summary>
    private const int DOWNLOAD_BUFFER_SIZE = 8192;

    /// <summary>
    ///     The amount of download threads to run at once.
    /// </summary>
    private const int MAX_DOWNLOAD_THREADS = 5;

    private IEnumerator? _consumeQueueCoroutine;

    private FileDownloader()
    {
    }

    public static FileDownloader Instance { get; } = new();

    private Queue<DownloadInfo> DownloadQueue { get; } = new();
    private List<DownloadInfo> ActiveDownloads { get; } = [];

    /// <summary>
    ///     Downloads a file asynchronously from the given URL and saves it to the specified file path using dotnet HttpClient.
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1</param>
    /// <param name="onComplete">Callback on successful completion</param>
    /// <param name="onError">Callback on error</param>
    public static void StartDownload(
        string url,
        string filePath,
        Action<float>? onProgress = null,
        Action<DownloadResult>? onComplete = null,
        Action<string>? onError = null)
    {
        Instance.AddToQueue(new DownloadInfo(url, filePath, onProgress, onComplete, onError));
    }

    /// <summary>
    ///     Adds a file to the download queue
    /// </summary>
    /// <param name="downloadInfo">Download options/data</param>
    public void AddToQueue(DownloadInfo downloadInfo)
    {
        // Check if download is already in queue
        var isAlreadyQueued = DownloadQueue.Any(d => d.ID == downloadInfo.ID) ||
                              ActiveDownloads.Any(d => d.ID == downloadInfo.ID);
        if (isAlreadyQueued)
            return;

        // Add download to the queue
        DownloadQueue.Enqueue(downloadInfo);

        // Start consuming the queue if it's not already running
        _consumeQueueCoroutine ??= Coroutines.Start(CoConsumeQueue());
    }

    private IEnumerator CoConsumeQueue()
    {
        while (DownloadQueue.Count > 0)
        {
            var downloadInfo = DownloadQueue.Dequeue();
            Coroutines.Start(CoDownload(downloadInfo));
            yield return null;
        }

        // All downloads are complete, stop the coroutine
        _consumeQueueCoroutine = null;
    }

    private IEnumerator CoDownload(DownloadInfo downloadInfo)
    {
        // Print download info
        LILogger.Info($"DOWNLOAD: {downloadInfo.DownloadURL} >> {downloadInfo.OutputFilePath}");
        ActiveDownloads.Add(downloadInfo);

        // Start the task on a background thread
        var progress = 0f;
        using var task = DownloadFileAsync(
            downloadInfo.DownloadURL,
            downloadInfo.OutputFilePath,
            v => progress = v);

        // Wait for the task to complete
        while (!task.IsCompleted)
        {
            // Report progress
            downloadInfo.OnProgress?.Invoke(progress);
            yield return null;
        }

        if (task.IsFaulted)
        {
            // Check for errors
            LILogger.Error(
                $"Error downloading file {downloadInfo.DownloadURL} >> {downloadInfo.OutputFilePath}:\n{task.Exception}");
            downloadInfo.OnError?.Invoke(task.Exception?.Message ?? "Unknown error");
        }
        else
        {
            // Log completion
            LILogger.Info($"DONE: {downloadInfo.OutputFilePath}");
            downloadInfo.OnComplete?.Invoke(task.Result);
        }

        // Remove from active downloads
        ActiveDownloads.Remove(downloadInfo);
    }


    /// <summary>
    ///     Background task to download a file using dotnet HttpClient.
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="filePath">Local file path to save to</param>
    /// <param name="onProgress">Callback on progress, value from 0 to 1. Warning: This is not called on the main Unity thread.</param>
    private static async Task<DownloadResult> DownloadFileAsync(
        string url,
        string filePath,
        Action<float>? onProgress)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (request, cert, _, errors) =>
        {
            // HACK: Bypass SSL error for LevelImposter API on mobile
            // This is due to an issue where root certificates are inaccessible at runtime with HttpClient on some mobile platforms
            if (cert?.Subject == "CN=storage.googleapis.com" &&
                request.RequestUri?.Host == "storage.googleapis.com" &&
                GameState.IsMobile)
                return true;

            if (cert?.Subject == "CN=levelimposter.net" &&
                request.RequestUri?.Host == "api.levelimposter.net" &&
                GameState.IsMobile)
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
        var tempFilePath = Path.GetTempFileName();
        await using var fileStream = new FileStream(
            tempFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            DOWNLOAD_BUFFER_SIZE,
            true);

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
        return new DownloadResult(new FileStore(filePath));
    }
}

public readonly record struct DownloadInfo(
    string DownloadURL,
    string OutputFilePath,
    Action<float>? OnProgress = null,
    Action<DownloadResult>? OnComplete = null,
    Action<string>? OnError = null
) : IIdentifiable
{
    // Uniquely identified by output file path to prevent duplicate downloads to the same location
    public string ID => OutputFilePath;
}

public readonly struct DownloadResult(FileStore store)
{
    public FileStore Store => store;
}