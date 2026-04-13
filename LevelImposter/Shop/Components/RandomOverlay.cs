using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LevelImposter.FileIO;
using LevelImposter.Shop.Transitions;
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
    
    // Animation Constants
    private const float ANIMATION_DURATION = 0.14f;
    private const float BACKGROUND_ALPHA = 0.7f;
    private readonly Vector3 OpenedSlideOffset = new Vector3(0, 0, -2.0f);
    private readonly Vector3 ClosedSlideOffset = new Vector3(0, -0.5f, -2.0f);

    private readonly Color ProgressGreen = new Color(0.38f, 0.80f, 0.32f);
    private readonly Color ProgressYellow = new Color(0.80f, 0.76f, 0.15f);
    private  readonly Color ProgressRed = new Color(0.81f, 0.22f, 0.22f);
    
    private const float DELTA_WEIGHT = 0.1f;
    
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
        gameObject.SetActive(true);
        SpriteOpacityTransition.Run(new TransitionParams<float>
        {
            TargetObject = backgroundShadow.Value,
            FromValue = 0,
            ToValue = BACKGROUND_ALPHA,
            Duration = ANIMATION_DURATION
        });
        MatOpacityTransition.Run(new TransitionParams<float>
        {
            TargetObject = gameObject,
            FromValue = 0,
            ToValue = 1,
            Duration = ANIMATION_DURATION
        });
        SlideTransition.Run(new TransitionParams<Vector3>
        {
            TargetObject =  gameObject,
            FromValue = ClosedSlideOffset,
            ToValue = OpenedSlideOffset,
            Curve = TransitionCurve.EaseInOut,
            Duration = ANIMATION_DURATION
        });
    }

    public void Close()
    {
        SpriteOpacityTransition.Run(new TransitionParams<float>
        {
            TargetObject = backgroundShadow.Value,
            FromValue = BACKGROUND_ALPHA,
            ToValue = 0,
            Duration = ANIMATION_DURATION
        });
        MatOpacityTransition.Run(new TransitionParams<float>
        {
            TargetObject = gameObject,
            FromValue = 1,
            ToValue = 0,
            Duration = ANIMATION_DURATION
        });
        SlideTransition.Run(new TransitionParams<Vector3>
        {
            TargetObject =  gameObject,
            FromValue = OpenedSlideOffset,
            ToValue = ClosedSlideOffset,
            Duration = ANIMATION_DURATION,
            Curve = TransitionCurve.EaseInOut,
            OnComplete = () => gameObject.SetActive(false)
        });
    }

    private void UpdateText()
    {
        var randomWeightPercent = Mathf.RoundToInt(_randomWeight * 100);
        primaryText.Value.SetText($"Weight: {randomWeightPercent}%");
    }

    private void UpdateProgressBar()
    {
        progressBar.Value.SetProgress(_randomWeight);
        
        // Transition color from red (0% weight) to yellow (50% weight) to green (100% weight)
        var color = _randomWeight < 0.5f ?
            Color.Lerp(ProgressRed, ProgressYellow, _randomWeight * 2) :
            Color.Lerp(ProgressYellow, ProgressGreen, (_randomWeight - 0.5f) * 2);
        progressBar.Value.SetColor(color);
    }
}