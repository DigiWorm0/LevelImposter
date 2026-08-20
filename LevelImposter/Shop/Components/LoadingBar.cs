using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader;
using LevelImposter.Builders;
using LevelImposter.Core.Translations;
using LevelImposter.Core.Utils;
using LevelImposter.Lobby.Sync;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop.Components;

public class LoadingBar(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private GameObject? _loadingBar;
    private TMP_Text? _mapText;
    private int _maxQueueSize = 1;
    private TMP_Text? _statusText;
    private bool _visible;

    public static LoadingBar? Instance { get; private set; }
    public static bool IsVisible => Instance?._visible ?? false;

    public void Awake()
    {
        Instance = this;

        _loadingBar = transform.Find("BarMask").Find("Bar").gameObject;
        _mapText = transform.Find("MapText").GetComponent<TMP_Text>();
        _statusText = transform.Find("StatusText").GetComponent<TMP_Text>();
    }

    public void OnDestroy()
    {
        Instance = null;

        _loadingBar = null;
        _mapText = null;
        _statusText = null;
    }

    /// <summary>
    ///     Runs the loading screen coroutine. Automatically manages the lifecycle of the loading bar.
    /// </summary>
    public static void Run()
    {
        // Create LoadingBar
        if (Instance == null)
            Instantiate(
                PackagedResources.LoadFromBundle<GameObject>("loadingbar"),
                DestroyableSingleton<HudManager>.Instance.transform
            );

        if (Instance == null)
            throw new Exception("Failed to create LoadingBar instance");

        // Check if visible
        if (!IsVisible)
        {
            // Apply initial state
            Instance.SetTitle(Translation.Get("loading.loading"));
            Instance.SetProgress(1);
            Instance.SetStatus(Translation.Get("loading.waiting_for_host"));

            // Start Coroutine
            Coroutines.Start(Instance.CoLoadingScreen());
        }
    }

    /// <summary>
    ///     Coroutine that displays the loading screen until map is built
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoLoadingScreen()
    {
        // Show Loading Screen
        Instance?.SetVisible(true);

        // Update Progress
        while (_visible)
        {
            var queueSize = GameState.LoadingAssetsCount;
            var downloadState =
                GameConfigurationSync.LobbyMapDownloader.CurrentDownloadState ??
                GameConfigurationSync.GameMapDownloader.CurrentDownloadState;

            // Approximate Progress
            if (queueSize > 0)
            {
                // Calculate Max Queue Size
                _maxQueueSize = Math.Max(_maxQueueSize, queueSize);

                // Calculate Progress
                var loadedCount = _maxQueueSize - queueSize;
                var progress = (float)loadedCount / _maxQueueSize;

                // Update UI
                var currentMap = GameConfiguration.CurrentMap ??
                                 GameConfiguration.CurrentLobbyMap;
                Instance?.SetTitle(!GameConfiguration.HideMapName
                    ? Translation.Get(
                        "loading.map_by_author",
                        currentMap?.name ?? "???",
                        currentMap?.authorName ?? "???")
                    : Translation.Get("loading.loading"));

                Instance?.SetProgress(progress);
                Instance?.SetStatus(
                    $"{Math.Round(progress * 100)}% <size=1.2>({loadedCount}/{_maxQueueSize})</size>"
                );
            }
            else if (downloadState != null)
            {
                Instance?.SetTitle(Translation.Get("loading.downloading_map"));
                Instance?.SetProgress(downloadState.Progress);
                Instance?.SetStatus($"{Math.Round(downloadState.Progress * 100)}%");
            }
            else
            {
                Instance?.SetTitle(Translation.Get("loading.waiting_for_host"));
                Instance?.SetProgress(1);
                Instance?.SetStatus("");
            }

            // Check if done
            var isSpritesLoading = SpriteLoader.Instance.QueueSize > 0;
            var isDownloading = downloadState != null;
            var isBuilding = MapBuilder.IsBuilding;
            if (!isSpritesLoading && !isDownloading && !isBuilding)
                break;

            yield return null;
        }

        // Reset Queue Size
        _maxQueueSize = 1;

        // Hide Loading Screen
        Instance?.SetVisible(false);
    }

    /// <summary>
    ///     Sets the name of the map being loaded
    /// </summary>
    /// <param name="mapName">Name of the map</param>
    public void SetTitle(string mapName)
    {
        _mapText?.SetText($"<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">{mapName}</font>");
    }

    /// <summary>
    ///     Sets the status text of the loading bar
    /// </summary>
    /// <param name="status">Text to display</param>
    public void SetStatus(string status)
    {
        _statusText?.SetText($"<font=\"VCR SDF\">{status}</font>");
    }

    /// <summary>
    ///     Sets the progress of the loading bar
    /// </summary>
    /// <param name="percent">Percentage of completion, from 0 to 1</param>
    public void SetProgress(float percent)
    {
        if (_loadingBar == null)
            return;

        _loadingBar.transform.localPosition = new Vector3(percent - 1, 0, 0);
    }

    /// <summary>
    ///     Sets the visibility of the loading bar
    /// </summary>
    /// <param name="visible">True iff the loading bar should be visible</param>
    public void SetVisible(bool visible)
    {
        // Me
        gameObject.SetActive(visible);
        _visible = visible;

        // Running Bean
        DestroyableSingleton<HudManager>.Instance.GameLoadAnimation.SetActive(visible);
    }
}