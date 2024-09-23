using System;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class LoadingBar(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private GameObject? _loadingBar;
    private TMP_Text? _mapText;
    private TMP_Text? _statusText;

    public static LoadingBar? Instance { get; private set; }

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

        //DontDestroyOnLoad(gameObject);
    }

    public void OnDestroy()
    {
        Instance = null;

        _loadingBar = null;
        _mapText = null;
        _statusText = null;
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
        gameObject.SetActive(visible);
    }
}