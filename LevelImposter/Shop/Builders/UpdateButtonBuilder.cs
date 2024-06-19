using LevelImposter.Core;
using System;
using TMPro;
using Twitch;
using UnityEngine;

namespace LevelImposter.Shop
{
    public static class UpdateButtonBuilder
    {
        private const string BUTTON_PATH = "MainMenuManager/MainUI/AspectScaler/LeftPanel/Main Buttons/BottomButtonBounds/ExitGameButton";

        private static Sprite? _btnSprite;
        private static Sprite? _btnHoverSprite;
        private static GenericPopup? _popupComponent;
        private static GameObject? _btnObj;

        public static void Build()
        {
            // Object
            GameObject buttonPrefab = GameObject.Find(BUTTON_PATH);
            _btnObj = UnityEngine.Object.Instantiate(buttonPrefab);
            _btnObj.transform.SetParent(VersionPatch.VersionObject?.transform);
            _btnObj.name = "button_LevelImposterUpdater";
            _btnObj.transform.localPosition = new Vector3(0.5f, 0.7f, 0);

            // Active/Inactive
            GameObject active = _btnObj.transform.Find("Highlight").gameObject;
            GameObject inactive = _btnObj.transform.Find("Inactive").gameObject;

            // Sprite
            Sprite btnSprite = GetSprite();
            Sprite btnHoverSprite = GetHoverSprite();
            float btnAspect = btnSprite.rect.height / btnSprite.rect.width;

            // Active
            SpriteRenderer btnRendererActive = active.GetComponent<SpriteRenderer>();
            btnRendererActive.sprite = btnHoverSprite;
            btnRendererActive.size = new Vector2(
                1.3f,
                1.3f * btnAspect
            );

            // Inactive
            SpriteRenderer btnRendererInactive = inactive.GetComponent<SpriteRenderer>();
            btnRendererInactive.sprite = btnSprite;
            btnRendererInactive.size = btnRendererActive.size;

            // Remove Text
            Transform textTransform = _btnObj.transform.FindChild("FontPlacer");
            UnityEngine.Object.Destroy(textTransform.gameObject);

            // Remove Aspect
            AspectPosition aspectComponent = _btnObj.GetComponent<AspectPosition>();
            UnityEngine.Object.Destroy(aspectComponent);

            // Popup
            GameObject popupPrefab = DestroyableSingleton<TwitchManager>.Instance.TwitchPopup.gameObject;
            GameObject popupObject = UnityEngine.Object.Instantiate(popupPrefab);
            _popupComponent = popupObject.GetComponent<GenericPopup>();
            TextMeshPro popupText = _popupComponent.TextAreaTMP;
            popupText.enableAutoSizing = false;

            // Button
            PassiveButton btnComponent = _btnObj.GetComponent<PassiveButton>();
            btnComponent.OnClick = new();
            btnComponent.OnClick.AddListener((Action)UpdateMod);

            // Box Collider
            BoxCollider2D btnCollider = _btnObj.GetComponent<BoxCollider2D>();
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

            GameObject confirmButton = _popupComponent.transform.FindChild("ExitGame").gameObject;
            confirmButton.SetActive(false);
            _popupComponent.Show("Updating...");
            _btnObj.SetActive(false);

            GitHubAPI.UpdateMod(() =>
            {
                confirmButton.SetActive(true);
                _popupComponent.Show("<color=green>Update complete!</color>\nPlease restart your game.");
            }, (error) =>
            {
                confirmButton.SetActive(true);
                _popupComponent.Show($"<color=red>Update failed!</color>\n<size=1.5>{error}\n<i>(You may have to update manually)</i></size>");
                _btnObj.SetActive(true);
            });
        }
    }
}
