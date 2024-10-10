using System;
using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using LevelImposter.AssetLoader;
using LevelImposter.Core;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

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
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

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
                MapUtils.LoadAssetBundle<GameObject>("loadingbar"),
                DestroyableSingleton<HudManager>.Instance.transform
            );

        // Check if asset bundle loaded
        if (Instance == null)
            throw new Exception("Failed to load LoadingBar asset bundle!");

        // Check if visible
        if (Instance._visible)
            return;

        // Start Coroutine
        Coroutines.Start(Instance.CoLoadingScreen());
    }

    /// <summary>
    ///     Coroutine that displayes the loading screen until map is built
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoLoadingScreen()
    {
        yield return null;

        // Objects
        var isFreeplay = GameState.IsInFreeplay;
        var currentMap = MapLoader.CurrentMap;
        var isFallback = MapLoader.IsFallback;

        // Show Loading Screen
        LILogger.Info($"Showing loading screen (Freeplay={isFreeplay})");

        // Set Map Name
        var mapName = "Loading...";
        if (currentMap != null && !isFallback)
            mapName = $"<color=#1a95d8>{currentMap.name}</color> by {currentMap.authorName}";
        Instance?.SetMapName(mapName);

        // Show Loading Screen
        Instance?.SetVisible(true);

        // Update Progress
        while (_visible)
        {
            // Approximate Progress
            if (SpriteLoader.Instance.QueueSize > 0)
            {
                // Calculate Max Queue Size
                _maxQueueSize = Math.Max(_maxQueueSize, SpriteLoader.Instance.QueueSize);
                if (_maxQueueSize == 0)
                    _maxQueueSize = 1;

                // Calculate Progress
                var loadedCount = _maxQueueSize - SpriteLoader.Instance.QueueSize;
                var progress = (float)loadedCount / _maxQueueSize;

                // Update UI
                Instance?.SetProgress(progress);
                Instance?.SetStatus(
                    $"{Math.Round(progress * 100)}% <size=1.2>({loadedCount}/{_maxQueueSize})</size>"
                );
            }
            else
            {
                Instance?.SetProgress(1);
                Instance?.SetStatus("waiting for host");
            }

            // Check if done
            var isSpritesLoading = SpriteLoader.Instance.QueueSize > 0;
            var isDownloading = MapSync.IsDownloadingMap;
            var isBuilding = LIShipStatus.GetInstanceOrNull()?.Builder.IsBuilding ?? false;
            if (!isSpritesLoading && !isDownloading && !isBuilding)
                break;

            yield return null;
        }

        // Hide Loading Screen
        Instance?.SetVisible(false);
    }

    /// <summary>
    ///     Sets the name of the map being loaded
    /// </summary>
    /// <param name="mapName">Name of the map</param>
    public void SetMapName(string mapName)
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

        // Running Bean
        DestroyableSingleton<HudManager>.Instance.GameLoadAnimation.SetActive(visible);

        // Set
        _visible = visible;
    }
}