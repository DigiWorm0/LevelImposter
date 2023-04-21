using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Twitch;
using TMPro;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class UpdateButtonBuilder
    {
        private static Sprite? _btnSprite;
        private static GenericPopup? _popupComponent;
        private static GameObject? _btnObj;

        public static void Build()
        {
            // Object
            GameObject buttonPrefab = GameObject.Find("ExitGameButton");
            _btnObj = UnityEngine.Object.Instantiate(buttonPrefab);
            _btnObj.name = "button_LevelImposterUpdater";
            _btnObj.transform.localPosition = new Vector3(4.25f, -2.3f, -1.0f);

            // Sprite
            Sprite btnSprite = GetSprite();
            float btnAspect = btnSprite.rect.height / btnSprite.rect.width;
            SpriteRenderer btnRenderer = _btnObj.GetComponent<SpriteRenderer>();
            btnRenderer.sprite = _btnSprite;
            btnRenderer.size = new Vector2(
                1.3f,
                1.3f * btnAspect
            );

            // Remove Text
            Transform textTransform = _btnObj.transform.GetChild(0);
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

            // Button Hover
            ButtonRolloverHandler btnRollover = _btnObj.GetComponent<ButtonRolloverHandler>();
            btnRollover.OverColor = new Color(1, 1, 1, 0.5f);

            // Box Collider
            BoxCollider2D btnCollider = _btnObj.GetComponent<BoxCollider2D>();
            btnCollider.size = btnRenderer.size;
        }

        private static Sprite GetSprite()
        {
            if (_btnSprite == null)
                _btnSprite = MapUtils.LoadSpriteResource("UpdateButton.png");
            if (_btnSprite == null)
                throw new Exception("The \"UpdateButton.png\" resource was not found in assembly");
            return _btnSprite;
        }

        private static void UpdateMod()
        {
            if (_popupComponent == null || _btnObj == null)
                return;

            GameObject confirmButton = _popupComponent.transform.FindChild("ExitGame").gameObject;
            confirmButton.SetActive(false);
            _popupComponent.Show("Updating...");
            _btnObj.SetActive(false);

            GitHubAPI.Instance?.UpdateMod(() =>
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
