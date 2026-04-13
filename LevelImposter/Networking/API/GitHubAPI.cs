using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.Core;
using LevelImposter.FileIO;
using UnityEngine;

namespace LevelImposter.Networking.API;

/// <summary>
///     API to recieve updates from GitHub.com
/// </summary>
public static class GitHubAPI
{
    private const string API_PATH = "https://api.github.com/repos/DigiWorm0/LevelImposter/releases?per_page=1";
    private const string UPDATE_BLACKLIST_FLAG = "[NoAutoUpdate]";
    
    private static readonly string UpdateWhitelistFlag = $"[AU={Application.version}]";

    /// <summary>
    ///     Gets the current path where the LevelImposter DLL is stored.
    /// </summary>
    /// <returns>String path where the LevelImposter DLL is stored.</returns>
    private static string GetDLLDirectory()
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
    public static void GetLatestRelease(Action<GitHubRelease> onSuccess, Action<string> onError)
    {
        LILogger.Info("Getting latest release info from GitHub");
        LILogger.Info(UpdateWhitelistFlag);
        HTTPHandler.RequestJSON<GitHubRelease[]>(API_PATH, result =>
        {
            // Validate response
            if (result.ErrorText != null)
                onError(result.ErrorText);

            else if (result.Data == null)
                onError("Invalid API response");

            else if (result.Data.Length == 0)
                onError("No releases found");

            else
                onSuccess(result.Data[0]);
        });
    }

    /// <summary>
    ///     Checks if a GitHubRelease can be updated to
    /// </summary>
    /// <param name="release">GitHub release object</param>
    /// <param name="reason">Reason for not being able to update</param>
    /// <returns>True if the release can be updated to</returns>
    [HideFromIl2Cpp]
    private static bool CanUpdateTo(GitHubRelease release, out string reason)
    {
        // Get version info
        var isCurrent = IsCurrent(release);
        var isWhitelisted = release.Body?.Contains(UpdateWhitelistFlag) ?? false;
        var isBlacklisted = release.Body?.Contains(UPDATE_BLACKLIST_FLAG) ?? false;
        var hasReleaseAssets = release.Assets?.Length > 0;
        

        // Set reason
        if (isCurrent)
            reason = "Already up-to-date";
        else if (!isWhitelisted)
            reason = "Incorrect Among Us version";
        else if (isBlacklisted)
            reason = "Auto-update to this version is disabled";
        else if (!hasReleaseAssets)
            reason = "No release assets found";
        else
            reason = "Unknown";

        // Return result
        return !isCurrent && isWhitelisted && !isBlacklisted && hasReleaseAssets;
    }

    /// <summary>
    ///     Checks if the release is the current version installed
    /// </summary>
    /// <param name="release">True if the release matches the current mod version</param>
    [HideFromIl2Cpp]
    public static bool IsCurrent(GitHubRelease release)
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
        
            // Prepare paths
            var tempDLLPath = Path.GetTempFileName();
            var activeDLLPath = GetDLLDirectory();
            var oldDLLPath = activeDLLPath + ".old";
            
            // Download DLL
            LILogger.Info($"Downloading DLL from {release}");
            var downloadURL = release.Assets?[0].BrowserDownloadURL ?? "";
            HTTPHandler.DownloadFile(
                downloadURL,
                tempDLLPath,
                null,
                (_) =>
                {
                    LILogger.Info("Replacing old DLL with new DLL");
                    try
                    {
                        // Move the active DLL to .old
                        if (File.Exists(oldDLLPath))
                            File.Delete(oldDLLPath);
                        File.Move(activeDLLPath, oldDLLPath);
                        
                        // Move the temp DLL to active
                        File.Move(tempDLLPath, activeDLLPath);
        
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
                });
        }, onError);
    }

    /// <summary>
    /// Represents a single release on a GitHub repository.
    /// </summary>
    [Serializable]
    public class GitHubRelease
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("body")] public string? Body { get; set; }
        [JsonPropertyName("assets")] public GitHubAsset[]? Assets { get; set; }

        public override string ToString()
        {
            return Name ?? base.ToString() ?? "GitHubRelease";
        }
    }

    /// <summary>
    ///   Represents a downloadable asset embedded in a GitHub release.
    ///   Typically, this would either be the LevelImposter DLL or a ZIP file containing the complete BepInEx mod.
    /// </summary>
    [Serializable]
    public class GitHubAsset
    {
        /// <summary>
        ///  URL to download the asset from.
        /// </summary>
        [JsonPropertyName("browser_download_url")] public string? BrowserDownloadURL { get; set; }
    }
}