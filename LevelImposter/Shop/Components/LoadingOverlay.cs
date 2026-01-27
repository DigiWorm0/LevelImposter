using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using TMPro;
using UnityEngine;

namespace LevelImposter.Shop;

public class LoadingOverlay(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    // Serialized fields
    public Il2CppReferenceField<ConnectionAnimation> connectionAnimation;
    public Il2CppReferenceField<GameObject> errorAnimation;
    public Il2CppReferenceField<TextMeshPro> titleText;
    public Il2CppReferenceField<TextMeshPro> descriptionText;
    public Il2CppReferenceField<ProgressBar> progressBar;
    public Il2CppReferenceField<GameObject> fullBackground;
    public Il2CppReferenceField<PassiveButton> closeButton;

    public bool PreventClose => fullBackground.Value.active && gameObject.activeSelf;
    
    // Among Us themed loading texts
    private readonly string[] _funLoadingTexts =
    [
        "Searching for habitable planets...",
        "Scanning for planetary systems...",
        "Searching dropship...",
        "Calibrating engines...",
        "Stabilizing reactor...",
        "Aligning telescope...",
        "Navigating asteroids...",
        "Diverting power...",
        "Doing card swipe...",
        "Fueling engines...",
        "Charting course...",
    ];

    public void Awake()
    {
        closeButton.Value.OnClick.AddListener((Action)Hide);   
    }

    public void Show(
        bool reverseAnimation = false,
        bool preventClose = false)
    {
        // Show Overlay
        gameObject.SetActive(true);
        fullBackground.Value.SetActive(preventClose);
        
        // Set Text
        RandomizeText();
        
        // Connection Animation
        connectionAnimation.Value.gameObject.SetActive(true);
        connectionAnimation.Value.SetReverse(reverseAnimation);
        errorAnimation.Value.SetActive(false);
        
        // Hide Close Button
        closeButton.Value.gameObject.SetActive(false);
        
        // Progress Bar
        SetProgress(null);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowError(string title, string description)
    {
        // Show Overlay
        gameObject.SetActive(true);
        
        // Set Text
        titleText.Value.text = title;
        descriptionText.Value.text = description;
        
        // Show Error Animation
        connectionAnimation.Value.gameObject.SetActive(false);
        errorAnimation.Value.SetActive(true);
        
        // Show Close Button if needed
        closeButton.Value.gameObject.SetActive(PreventClose);
    }
    
    public void RandomizeText(string subtitle = "(Fetching maps)")
    {
        var randomIndex = UnityEngine.Random.Range(0, _funLoadingTexts.Length);
        SetText(_funLoadingTexts[randomIndex], subtitle);
    }

    public void SetProgress(float? progress)
    {
        if (progress == null)
        {
            progressBar.Value.gameObject.SetActive(false);
            return;
        }

        progressBar.Value.SetProgress(progress.Value);
        progressBar.Value.gameObject.SetActive(true);
    }
    
    public void SetText(string title, string description)
    {
        titleText.Value.text = title;
        descriptionText.Value.text = description;
    }
}