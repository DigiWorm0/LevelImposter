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

namespace LevelImposter.Shop
{
    /// <summary>
    /// API to recieve updates from GitHub.com
    /// </summary>
    public class GitHubAPI : MonoBehaviour
    {
        public const string API_PATH = "https://api.github.com/repos/DigiWorm0/LevelImposter/releases?per_page=1";

        public static GitHubAPI Instance;

        public GitHubAPI(IntPtr intPtr) : base(intPtr)
        {
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

        /// <summary>
        /// Runs an Async HTTP Request on a specific url.
        /// Handles and logs any errors.
        /// </summary>
        /// <param name="url">URL to request</param>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        public void Request(string url, Action<string> onSucccess, Action<string> onError)
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
        public IEnumerator CoRequest(string url, Action<string> onSuccess, Action<string> onError)
        {
            LILogger.Info("GET: " + url);
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            LILogger.Info("RES: " + request.responseCode);

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                LILogger.Error(request.error);
                onError(request.error);
            }
            else
                onSuccess(request.downloadHandler.text);
            request.Dispose();
        }

        /// <summary>
        /// Gets the current path where the LevelImposter DLL is stored.
        /// </summary>
        /// <returns>String path where the LevelImposter DLL is stored.</returns>
        public string GetDLLDirectory()
        {
            string gameDir = System.Reflection.Assembly.GetAssembly(typeof(LevelImposter)).Location;
            return gameDir;
        }

        /// <summary>
        /// Gets the latest release data from GitHub
        /// </summary>
        /// <param name="onSuccess">Callback on success</param>
        /// <param name="onError">Callback on error</param>
        public void GetLatestRelease(Action<GHRelease> onSuccess, Action<string> onError)
        {
            Request(API_PATH, (string json) =>
            {
                GHRelease[] response = JsonSerializer.Deserialize<GHRelease[]>(json);
                onSuccess(response[0]);
            }, onError);
        }

        /// <summary>
        /// Checks if the GitHub release matches
        /// the current release version
        /// </summary>
        /// <param name="release">Release data to check</param>
        /// <returns>True if the release matches the current version</returns>
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
        public void UpdateMod(Action onSuccess, Action<string> onError)
        {
            GetLatestRelease((release) =>
            {
                string dllPath = GetDLLDirectory();
                string dllTempPath = dllPath + ".part";

                if (File.Exists(dllTempPath))
                    File.Delete(dllTempPath);

                string downloadURL = release.assets[0].browser_download_url;
                Request(downloadURL, (string dllString) =>
                {
                    using (var fileStream = File.Create(dllTempPath))
                    {
                        try
                        {
                            byte[] dllBytes = Encoding.ASCII.GetBytes(dllString);
                            fileStream.Write(dllBytes);
                            File.Move(dllTempPath, dllPath, true);
                            onSuccess();
                        }
                        catch (Exception e)
                        {
                            onError(e.Message);
                        }
                    }
                }, onError);
            }, onError);
        }
    }
}