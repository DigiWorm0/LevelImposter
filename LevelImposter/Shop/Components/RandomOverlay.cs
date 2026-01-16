using System;
using LevelImposter.FileIO;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     Represents the overlay used on Map Banners to control the random weight of a map
/// </summary>
public class RandomOverlay(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    private const float DELTA_WEIGHT = 0.1f;
    private PassiveButton? _exitButton;

    private string? _mapID;
    private PassiveButton? _minusButton;
    private PassiveButton? _plusButton;
    private float _randomWeight;
    private TMP_Text? _text;

    public void Awake()
    {
        _text = transform.Find("Text")?.GetComponent<TMP_Text>();
        _plusButton = transform.Find("PlusButton")?.GetComponent<PassiveButton>();
        _minusButton = transform.Find("MinusButton")?.GetComponent<PassiveButton>();
        _exitButton = transform.Find("ExitButton")?.GetComponent<PassiveButton>();
    }

    public void Start()
    {
        // Buttons
        _plusButton?.OnClick.AddListener((Action)OnPlus);
        _minusButton?.OnClick.AddListener((Action)OnMinus);
        _exitButton?.OnClick.AddListener((Action)OnExit);
    }

    public void OnDestroy()
    {
        _mapID = null;
        _randomWeight = 0;
        _text = null;
        _plusButton = null;
        _minusButton = null;
        _exitButton = null;
    }

    /// <summary>
    ///     Opens the overlay for the specified map
    /// </summary>
    /// <param name="mapID">ID of e map to open the overlay for</param>
    public void Open(string mapID)
    {
        _mapID = mapID;
        _randomWeight = ConfigAPI.GetMapWeight(_mapID);
        gameObject.SetActive(true);
        UpdateText();
    }

    /// <summary>
    ///     Updates the text of the overlay
    /// </summary>
    private void UpdateText()
    {
        var randomWeightPercent = Mathf.RoundToInt(_randomWeight * 100);
        _text?.SetText($"<b>Weight: {randomWeightPercent}%</b>");
    }

    /// <summary>
    ///     Fired on plus button click
    /// </summary>
    private void OnPlus()
    {
        _randomWeight = Mathf.Clamp(_randomWeight + DELTA_WEIGHT, 0, 1);
        ConfigAPI.SetMapWeight(_mapID ?? "", _randomWeight);
        ShopManager.RandomizeMapOnClose();
        UpdateText();
    }

    /// <summary>
    ///     Fired on minus button click
    /// </summary>
    private void OnMinus()
    {
        _randomWeight = Mathf.Clamp(_randomWeight - DELTA_WEIGHT, 0, 1);
        ConfigAPI.SetMapWeight(_mapID ?? "", _randomWeight);
        ShopManager.RandomizeMapOnClose();
        UpdateText();
    }

    /// <summary>
    ///     Fired on exit button click
    /// </summary>
    private void OnExit()
    {
        ConfigAPI.Save();
        gameObject.SetActive(false);
    }
}