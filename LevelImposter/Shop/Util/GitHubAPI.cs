using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     API to recieve updates from GitHub.com
/// </summary>
public static class GitHubAPI
{
    public const string API_PATH = "https://api.github.com/repos/DigiWorm0/LevelImposter/releases?per_page=1";
    public const string DEV_VERSION_FLAG = "dev";
    public const string UPDATE_BLACKLIST_FLAG = "[NoAutoUpdate]";
    public static readonly string UPDATE_WHITELIST_FLAG = $"[AU={Application.version}]";

    /// <summary>
    ///     Gets the current path where the LevelImposter DLL is stored.
    /// </summary>
    /// <returns>String path where the LevelImposter DLL is stored.</returns>
    public static string GetDLLDirectory()
    {
        var gameDir = Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
        return gameDir;
    }

    /// <summary>
    ///     Gets the latest release info from GitHub
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void GetLatestRelease(Action<GHRelease> onSuccess, Action<string> onError)
    {
        LILogger.Info("Getting latest release info from GitHub");
        LILogger.Info(UPDATE_WHITELIST_FLAG);
        HTTPHandler.Instance?.Request(API_PATH, json =>
        {
            var responses = JsonSerializer.Deserialize<GHRelease[]>(json);
            if (responses != null && responses.Length > 0)
                onSuccess(responses[0]);
            else
                onError("Invalid API response");
        }, onError);
    }

    /// <summary>
    ///     Checks if a GHRelease can be updated to
    /// </summary>
    /// <param name="release">GitHub release object</param>
    /// <param name="reason">Reason for not being able to update</param>
    /// <returns>True if the release can be updated to</returns>
    [HideFromIl2Cpp]
    public static bool CanUpdateTo(GHRelease release, out string reason)
    {
        // Get version info
        var versionString = release.Name?.Split(" ")[1];
        var isCurrent = IsCurrent(release);
        var isDevVersion = versionString?.Contains(DEV_VERSION_FLAG) ?? false;
        var isWhitelisted = release.Body?.Contains(UPDATE_WHITELIST_FLAG) ?? false;
        var isBlacklisted = release.Body?.Contains(UPDATE_BLACKLIST_FLAG) ?? false;
        var hasReleaseAssets = release.Assets?.Length > 0;

        // Set reason
        if (isCurrent)
            reason = "Already up-to-date";
        else if (isDevVersion)
            reason = "You're on a dev version";
        else if (!isWhitelisted)
            reason = "Incorrect Among Us version";
        else if (isBlacklisted)
            reason = "Auto-update to this version is disabled";
        else if (!hasReleaseAssets)
            reason = "No release assets found";
        else
            reason = "Unknown";

        // Return result
        return !isCurrent && !isDevVersion && isWhitelisted && !isBlacklisted && hasReleaseAssets;
    }

    /// <summary>
    ///     Checks if the release is the current version installed
    /// </summary>
    /// <param name="release">True if the release matches the current mod version</param>
    [HideFromIl2Cpp]
    public static bool IsCurrent(GHRelease release)
    {
        var versionString = release.Name?.Split(" ")[1];
        return versionString == LevelImposter.DisplayVersion;
    }

    /// <summary>
    ///     Downloads and updates the DLL to the latest version
    /// </summary>
    /// <param name="onSuccess">Callback on success</param>
    /// <param name="onError">Callback on error</param>
    [HideFromIl2Cpp]
    public static void UpdateMod(Action onSuccess, Action<string> onError)
    {
        LILogger.Info("Updating mod from GitHub");
        GetLatestRelease(release =>
        {
            // Check if update is available
            if (!CanUpdateTo(release, out var reason))
            {
                var errorMsg = $"Auto-update to {release} is unavailable:\n{reason}";
                LILogger.Error(errorMsg);
                onError(errorMsg);
                return;
            }

            // Download DLL
            LILogger.Info($"Downloading DLL from {release}");
            var downloadURL = release.Assets?[0].BrowserDownloadURL ?? "";
            HTTPHandler.Instance?.Request(downloadURL, dllBytes =>
            {
                LILogger.Info($"Saving {dllBytes.Length / 1024}kb DLL to local filesystem");
                try
                {
                    // Get DLL path
                    var dllPath = GetDLLDirectory();
                    var dllOldPath = dllPath + ".old";

                    // Move old DLL
                    if (File.Exists(dllOldPath))
                        File.Delete(dllOldPath);
                    File.Move(dllPath, dllOldPath);

                    // Write new DLL
                    using var fileStream = File.Create(dllPath);
                    fileStream.Write(dllBytes, 0, dllBytes.Length);

                    // Clear cache
                    FileCache.Clear();

                    // Log success
                    LILogger.Info("Update complete");
                    onSuccess();
                }
                catch (Exception e)
                {
                    LILogger.Error(e);
                    onError(e.Message);
                }
            }, onError);
        }, onError);
    }
}