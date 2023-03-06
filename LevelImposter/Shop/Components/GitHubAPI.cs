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
    /// API to recieve updates from GitHub.com
    /// </summary>
    public class GitHubAPI : MonoBehaviour
    {
        public GitHubAPI(IntPtr intPtr) : base(intPtr)
        {
        }

        public const string API_PATH = "https://api.github.com/repos/DigiWorm0/LevelImposter/releases?per_page=5";
        public const string UPDATE_FORBIDDEN_FLAG = "[NoAutoUpdate]";

        public static GitHubAPI? Instance = null;

        /// <summary>
        /// Runs an Async HTTP Request on a specific url.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public void Request(string url, Action<byte[]> onSucccess, Action<string> onError)
        {
            StartCoroutine(CoRequest(url, onSucccess, onError).WrapToIl2Cpp());
        }

        /// <summary>
        /// Runs an Async HTTP Request on a
        /// specific url with a Unity Coroutine.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public IEnumerator CoRequest(string url, Action<byte[]>? onSuccess, Action<string>? onError)
        {
            {
                LILogger.Info($"GET: {url}");
                UnityWebRequest request = UnityWebRequest.Get(url); // Doesn't extend IDisposable
                yield return request.SendWebRequest();
                LILogger.Info($"RES: {request.responseCode}");

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    LILogger.Error(request.error);
                    if (onError != null)
                        onError(request.error);
                }
                else if (onSuccess != null)
                {
                    onSuccess(request.downloadHandler.data);
                }
                request.Dispose();
                request = null;
                onSuccess = null;
                onError = null;
            }
        }

        /// <summary>
        /// Gets the current path where the LevelImposter DLL is stored.
        /// </summary>
        /// <returns>String path where the LevelImposter DLL is stored.</returns>
        public string GetDLLDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter))?.Location ?? "/";
            return gameDir;
        }

        /// <summary>
        /// Gets the latest release info from GitHub
        /// </summary>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        private void GetLatestReleases(Action<GHRelease[]> onSuccess, Action<string> onError)
        {
            LILogger.Info("Getting latest release info from GitHub");
            Request(API_PATH, (byte[] rawData) =>
            {
                string json = Encoding.UTF8.GetString(rawData);
                GHRelease[]? response = JsonSerializer.Deserialize<GHRelease[]>(json);
                if (response != null)
                    onSuccess(response);
                else
                    onError("Invalid API response");
            }, onError);
        }

        /// <summary>
        /// Gets the latest release data from GitHub
        /// </summary>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public void GetLatestRelease(Action<GHRelease> onSuccess, Action<string> onError)
        {
            GetLatestReleases((releases) =>
            {
                onSuccess(releases[0]);
            }, onError);
        }

        /// <summary>
        /// Checks release chain if update is forbidden
        /// </summary>
        /// <param name="releases">List of releases in order of relevancy</param>
        /// <returns>TRUE if the update is forbidden. FALSE otherwise</returns>
        private bool IsUpdateForbidden(GHRelease[] releases)
        {
            foreach (GHRelease release in releases)
            {
                if (IsCurrent(release))
                    return false;
                if (release.body.Contains(UPDATE_FORBIDDEN_FLAG))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the GitHub release matches
        /// the current release version
        /// </summary>
        /// <param name="release">Release data to check</param>
        /// <returns>True if the release matches the current version</returns>
        [HideFromIl2Cpp]
        public bool IsCurrent(GHRelease release)
        {
            string versionString = release.name.Split(" ")[1];
            return versionString == LevelImposter.Version;
        }

        /// <summary>
        /// Downloads and updates the DLL to the latest version
        /// </summary>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        [HideFromIl2Cpp]
        public void UpdateMod(Action onSuccess, Action<string> onError)
        {
            LILogger.Info("Updating mod from GitHub");
            GetLatestReleases((releases) =>
            {
                GHRelease release = releases[0];
                LILogger.Info($"Downloading DLL from {release}");
                if (release.assets.Length <= 0)
                {
                    string errorMsg = $"No release assets were found for {release}";
                    LILogger.Error(errorMsg);
                    onError(errorMsg);
                    return;
                }
                if (IsUpdateForbidden(releases))
                {
                    string errorMsg = $"Auto-update to {release} is unavailable.";
                    LILogger.Error(errorMsg);
                    onError(errorMsg);
                    return;
                }
                string downloadURL = release.assets[0].browser_download_url;
                Request(downloadURL, (byte[] dllBytes) =>
                {
                    LILogger.Info($"Saving {dllBytes.Length / 1024}kb DLL to local filesystem");
                    try
                    {
                        string dllPath = GetDLLDirectory();
                        string dllOldPath = dllPath + ".old";

                        if (File.Exists(dllOldPath))
                            File.Delete(dllOldPath);
                        File.Move(dllPath, dllOldPath);

                        using (FileStream fileStream = File.Create(dllPath))
                        {
                            fileStream.Write(dllBytes, 0, dllBytes.Length);
                        }

                        ThumbnailFileAPI.Instance?.DeleteAll();

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