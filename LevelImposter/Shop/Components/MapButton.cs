using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class MapButton : MonoBehaviour
    {
        private TMPro.TMP_Text titleText;
        private TMPro.TMP_Text descriptionText;
        private TMPro.TMP_Text authorText;
        private SpriteRenderer spriteRenderer;
        private PassiveButton button;
        private LIMetadata metadata;
        private bool isDownloaded
        {
            get { return MapLoader.Exists(metadata.id); }
        }

        public void SetMap(LIMetadata metadata)
        {
            titleText = transform.Find("Title").GetComponent<TMPro.TMP_Text>();
            descriptionText = transform.Find("Description").GetComponent<TMPro.TMP_Text>();
            authorText = transform.Find("Author").GetComponent<TMPro.TMP_Text>();
            spriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
            button = transform.GetComponent<PassiveButton>();
            
            this.metadata = metadata;

            titleText.text = metadata.name;
            descriptionText.text = metadata.description;
            authorText.text = metadata.authorName;

            UpdateButton();

            gameObject.SetActive(true);
        }

        public void UpdateButton()
        {
            button.OnClick.RemoveAllListeners();
            if (isDownloaded)
            {
                spriteRenderer.color = Color.red;
                button.OnClick.AddListener((Action)DeleteMap);
            }
            else
            {
                spriteRenderer.color = Color.green;
                button.OnClick.AddListener((Action)DownloadMap);
            }
        }

        public void DownloadMap()
        {
            button.gameObject.SetActive(false);
            ShopManager.Instance.DownloadMap(metadata.id, OnDownloadFinish);
        }

        public void OnDownloadFinish()
        {
            MapLoader.LoadMap(metadata.id);
            UpdateButton();
            button.gameObject.SetActive(true);
        }

        public void DeleteMap()
        {
            ShopManager.Instance.DeleteMap(metadata.id);
            UpdateButton();
        }
    }
}