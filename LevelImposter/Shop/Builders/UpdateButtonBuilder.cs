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
        private static Sprite? _buttonSprite;
        private static GenericPopup? _popupComponent;
        private static GameObject? _buttonObj;

        public static void Build()
        {
            // Object
            GameObject buttonPrefab = GameObject.Find("ExitGameButton");
            _buttonObj = UnityEngine.Object.Instantiate(buttonPrefab);
            _buttonObj.name = "button_LevelImposterUpdater";
            _buttonObj.transform.localPosition = new Vector3(4.25f, -2.3f, -1.0f);

            // Sprite
            if (_buttonSprite == null)
                _buttonSprite = SpriteLoader.Instance?.LoadSprite(Properties.Resources.updateButton);
            float btnAspect = _buttonSprite != null ? _buttonSprite.rect.height / _buttonSprite.rect.width : 1.0f;
            SpriteRenderer btnRenderer = _buttonObj.GetComponent<SpriteRenderer>();
            btnRenderer.sprite = _buttonSprite;
            btnRenderer.size = new Vector2(
                1.3f,
                1.3f * btnAspect
            );

            // Remove Text
            Transform textTransform = _buttonObj.transform.GetChild(0);
            UnityEngine.Object.Destroy(textTransform.gameObject);

            // Remove Aspect
            AspectPosition aspectComponent = _buttonObj.GetComponent<AspectPosition>();
            UnityEngine.Object.Destroy(aspectComponent);

            // Popup
            GameObject popupPrefab = TwitchManager.Instance.TwitchPopup.gameObject;
            GameObject popupObject = UnityEngine.Object.Instantiate(popupPrefab);
            _popupComponent = popupObject.GetComponent<GenericPopup>();
            TextMeshPro popupText = _popupComponent.TextAreaTMP;
            popupText.enableAutoSizing = false;

            // Button
            PassiveButton btnComponent = _buttonObj.GetComponent<PassiveButton>();
            btnComponent.OnClick = new();
            btnComponent.OnClick.AddListener((Action)UpdateMod);

            // Button Hover
            ButtonRolloverHandler btnRollover = _buttonObj.GetComponent<ButtonRolloverHandler>();
            btnRollover.OverColor = new Color(1, 1, 1, 0.5f);

            // Box Collider
            BoxCollider2D btnCollider = _buttonObj.GetComponent<BoxCollider2D>();
            btnCollider.size = btnRenderer.size;
        }

        private static void UpdateMod()
        {
            if (_popupComponent == null || _buttonObj == null)
                return;

            GameObject confirmButton = _popupComponent.transform.FindChild("ExitGame").gameObject;
            confirmButton.SetActive(false);
            _popupComponent.Show("Updating...");
            _buttonObj.SetActive(false);

            GitHubAPI.Instance?.UpdateMod(() =>
            {
                confirmButton.SetActive(true);
                _popupComponent.Show("<color=green>Update complete!</color>\nPlease restart your game.");
            }, (error) =>
            {
                confirmButton.SetActive(true);
                _popupComponent.Show($"<color=red>Update failed!</color>\n<size=1.5>{error}\n<i>(You may have to update manually)</i></size>");
                _buttonObj.SetActive(true);
            });
        }
    }
}
