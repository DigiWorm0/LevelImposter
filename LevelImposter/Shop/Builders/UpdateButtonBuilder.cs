using System;
using LevelImposter.Core;
using LevelImposter.FileIO;
using LevelImposter.Networking.API;
using Twitch;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LevelImposter.Shop;

public static class UpdateButtonBuilder
{
    private const string BUTTON_PATH =
        "MainMenuManager/MainUI/AspectScaler/LeftPanel/Main Buttons/BottomButtonBounds/ExitGameButton";

    private static Sprite? _btnSprite;
    private static Sprite? _btnHoverSprite;
    private static GenericPopup? _popupComponent;
    private static GameObject? _btnObj;

    public static void Build()
    {
        // Find the VersionShower
        var versionShower = GameObject.FindObjectOfType<VersionShower>();
        if (versionShower == null)
            throw new Exception("Could not find VersionShower in scene");
        
        // Find Button Prefab to Clone
        var buttonPrefab = GameObject.Find(BUTTON_PATH);
        if (buttonPrefab == null)
            throw new Exception($"Could not find button prefab at path: {BUTTON_PATH}");
        
        // Create Button Object
        _btnObj = Object.Instantiate(buttonPrefab, versionShower.transform.parent);
        _btnObj.name = "button_LevelImposterUpdater";
        _btnObj.transform.localScale = new Vector3(1.1f, 1.1f, 1.0f);
        // _btnObj.transform.localPosition = new Vector3(-2.5f, 0.05f, 0);

        // Active/Inactive
        var active = _btnObj.transform.Find("Highlight").gameObject;
        var inactive = _btnObj.transform.Find("Inactive").gameObject;

        // Sprite
        var btnSprite = GetSprite();
        var btnHoverSprite = GetHoverSprite();
        var btnAspect = btnSprite.rect.height / btnSprite.rect.width;

        // Active
        var btnRendererActive = active.GetComponent<SpriteRenderer>();
        btnRendererActive.sprite = btnHoverSprite;
        btnRendererActive.size = new Vector2(
            1.3f,
            1.3f * btnAspect
        );

        // Inactive
        var btnRendererInactive = inactive.GetComponent<SpriteRenderer>();
        btnRendererInactive.sprite = btnSprite;
        btnRendererInactive.size = btnRendererActive.size;

        // Remove Text
        var textTransform = _btnObj.transform.FindChild("FontPlacer");
        Object.Destroy(textTransform.gameObject);

        // Update Aspect Position
        var aspectComponent = _btnObj.GetComponent<AspectPosition>();
        aspectComponent.DistanceFromEdge = new Vector3(2.5f, -1.5f, 5.0f);
        aspectComponent.Alignment = AspectPosition.EdgeAlignments.Center;

        // Popup
        var popupPrefab = DestroyableSingleton<TwitchManager>.Instance.TwitchPopup.gameObject;
        var popupObject = Object.Instantiate(popupPrefab);
        _popupComponent = popupObject.GetComponent<GenericPopup>();
        var popupText = _popupComponent.TextAreaTMP;
        popupText.enableAutoSizing = false;

        // Button
        var btnComponent = _btnObj.GetComponent<PassiveButton>();
        btnComponent.OnClick = new Button.ButtonClickedEvent();
        btnComponent.OnClick.AddListener((Action)UpdateMod);

        // Box Collider
        var btnCollider = _btnObj.GetComponent<BoxCollider2D>();
        btnCollider.size = btnRendererActive.size;
    }

    private static Sprite GetSprite()
    {
        if (_btnSprite == null)
            _btnSprite = MapUtils.LoadSpriteResource("UpdateButton.png");
        if (_btnSprite == null)
            throw new Exception("The \"UpdateButton.png\" resource was not found in assembly");
        return _btnSprite;
    }

    private static Sprite GetHoverSprite()
    {
        if (_btnHoverSprite == null)
            _btnHoverSprite = MapUtils.LoadSpriteResource("UpdateButtonHover.png");
        if (_btnHoverSprite == null)
            throw new Exception("The \"UpdateButtonHover.png\" resource was not found in assembly");
        return _btnHoverSprite;
    }

    private static void UpdateMod()
    {
        if (_popupComponent == null || _btnObj == null)
            return;

        var confirmButton = _popupComponent.transform.FindChild("ExitGame").gameObject;
        confirmButton.SetActive(false);
        _popupComponent.Show("Updating...");
        _btnObj.SetActive(false);

        GitHubAPI.UpdateMod(() =>
        {
            confirmButton.SetActive(true);
            _popupComponent.Show("<color=green>Update complete!</color>\nPlease restart your game.");
        }, error =>
        {
            confirmButton.SetActive(true);
            _popupComponent.Show(
                $"<color=red>Update failed!</color>\n<size=1.5>{error}\n<i>(You may have to update manually)</i></size>");
            _btnObj.SetActive(true);
        });
    }
}