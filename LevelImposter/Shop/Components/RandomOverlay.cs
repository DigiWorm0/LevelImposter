using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.FileIO;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

/// <summary>
///     Represents the overlay used on Map Banners to control the random weight of a map
/// </summary>
public class RandomOverlay(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    // Serialized Fields
    public Il2CppReferenceField<TextMeshPro> primaryText;
    public Il2CppReferenceField<PassiveButton> plusButton;
    public Il2CppReferenceField<PassiveButton> minusButton;
    public Il2CppReferenceField<PassiveButton> doneButton;
    public Il2CppReferenceField<GameObject> backgroundShadow;
    public Il2CppReferenceField<ProgressBar> progressBar;
    
    private const float DELTA_WEIGHT = 0.1f;
    private const float ANIMATION_DURATION = 0.2f;
    private const float BACKGROUND_ALPHA = 0.7f;
    
    private string? _mapID;
    private float _randomWeight;

    public void Awake()
    {
        plusButton.Value.OnClick.AddListener((Action)OnPlusClick);
        minusButton.Value.OnClick.AddListener((Action)OnMinusClick);
        doneButton.Value.OnClick.AddListener((Action)Close);
    }
    
    private void OnPlusClick() => ChangeMapWeight(DELTA_WEIGHT);
    private void OnMinusClick() => ChangeMapWeight(-DELTA_WEIGHT);
    private void ChangeMapWeight(float amount)
    {
        if (_mapID == null)
            throw new Exception("MapID is null");
        
        _randomWeight = Mathf.Clamp(_randomWeight + amount, 0, 1);
        ConfigAPI.SetMapWeight(_mapID, _randomWeight);
        ShopManager.Instance?.RandomizeMapOnClose();
        UpdateText();
        UpdateProgressBar();
    }
    
    public void SetMapID(string mapID)
    {
        _mapID = mapID;
        _randomWeight = ConfigAPI.GetMapWeight(_mapID);
        UpdateText();
        UpdateProgressBar();
    }
    
    public void Open()
    {
        // TODO: Add animation
        gameObject.SetActive(true);
        TransitionHelper.Fade(backgroundShadow.Value, 0, BACKGROUND_ALPHA, ANIMATION_DURATION);
    }

    public void Close()
    {
        // TODO: Add animation
        gameObject.SetActive(false);
        TransitionHelper.Fade(backgroundShadow.Value, BACKGROUND_ALPHA, 0, ANIMATION_DURATION);
    }

    private void UpdateText()
    {
        var randomWeightPercent = Mathf.RoundToInt(_randomWeight * 100);
        primaryText.Value.SetText($"Weight: {randomWeightPercent}%");
    }

    private void UpdateProgressBar()
    {
        progressBar.Value.SetProgress(_randomWeight);
    }
}