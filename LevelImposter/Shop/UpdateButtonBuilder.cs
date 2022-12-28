using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Twitch;
using TMPro;

namespace LevelImposter.Shop
{
    public static class UpdateButtonBuilder
    {
        private static Texture2D _buttonTex;
        private static GenericPopup _popupComponent;

        public static void Build()
        {
            // Object
            GameObject buttonPrefab = GameObject.Find("ExitGameButton");
            GameObject buttonObj = GameObject.Instantiate(buttonPrefab);
            buttonObj.name = "button_LevelImposterUpdater";
            buttonObj.transform.localPosition = new Vector3(4.25f, -2.3f, -1.0f);

            // Sprite
            Sprite btnSprite = GetSprite();
            float btnAspect = btnSprite.rect.height / btnSprite.rect.width;
            SpriteRenderer btnRenderer = buttonObj.GetComponent<SpriteRenderer>();
            btnRenderer.sprite = btnSprite;
            btnRenderer.size = new Vector2(
                1.3f,
                1.3f * btnAspect
            );

            // Remove Text
            Transform textTransform = buttonObj.transform.GetChild(0);
            UnityEngine.Object.Destroy(textTransform.gameObject);

            // Remove Aspect
            AspectPosition aspectComponent = buttonObj.GetComponent<AspectPosition>();
            UnityEngine.Object.Destroy(aspectComponent);

            // Popup
            GameObject popupPrefab = TwitchManager.Instance.TwitchPopup.gameObject;
            GameObject popupObject = UnityEngine.Object.Instantiate(popupPrefab);
            _popupComponent = popupObject.GetComponent<GenericPopup>();
            TextMeshPro popupText = _popupComponent.TextAreaTMP;
            popupText.enableAutoSizing = false;

            // Button
            PassiveButton btnComponent = buttonObj.GetComponent<PassiveButton>();
            btnComponent.OnClick = new();
            btnComponent.OnClick.AddListener((Action)Update);

        }

        private static void Update()
        {
            if (_popupComponent == null)
                return;

            GameObject confirmButton = _popupComponent.transform.FindChild("ExitGame").gameObject;
            confirmButton.SetActive(false);
            _popupComponent.Show("Updating...");

            GitHubAPI.Instance.UpdateToLatest(() => {
                confirmButton.SetActive(true);
                _popupComponent.Show("<color=green>Update complete!</color>\nPlease restart your game.");
            }, (error) => {
                confirmButton.SetActive(true);
                _popupComponent.Show($"<color=red>Update failed!</color>\n<size=1>${error}\nYou may have to update manually.</size>");
            });
        }

        private static Sprite GetSprite()
        {
            if (_buttonTex == null)
            {
                _buttonTex = new Texture2D(1, 1);
                ImageConversion.LoadImage(_buttonTex, Properties.Resources.updateButton);
            }
            Sprite sprite = Sprite.Create(
                _buttonTex,
                new Rect(0, 0, _buttonTex.width, _buttonTex.height),
                new Vector2(0.5f, 0.5f),
                100.0f
            );
            return sprite;
        }
    }
}
