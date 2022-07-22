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
        private bool isDownloading = false;
        private bool isHovering = false;
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
            button.OnMouseOut.AddListener((Action)OnMouseOut);
            button.OnMouseOver.AddListener((Action)OnMouseOver);

            gameObject.SetActive(true);
        }

        public void UpdateButton()
        {
            button.OnClick.RemoveAllListeners();
            if (isDownloading)
            {
                spriteRenderer.color = Color.yellow;
            }
            else if (isDownloaded)
            {
                spriteRenderer.color = Color.red;
                button.OnClick.AddListener((Action)DeleteMap);
            }
            else
            {
                if (isHovering)
                    spriteRenderer.color = Color.green;
                else
                    spriteRenderer.color = Color.white;
                button.OnClick.AddListener((Action)DownloadMap);
            }
        }

        public void OnMouseOver()
        {
            isHovering = true;
            UpdateButton();
        }
        public void OnMouseOut()
        {
            isHovering = false;
            UpdateButton();
        }

        public void DownloadMap()
        {
            isDownloading = true;
            ShopManager.Instance.DownloadMap(metadata.id, OnDownloadFinish);
            UpdateButton();
        }

        public void OnDownloadFinish()
        {
            isDownloading = false;
            MapLoader.LoadMap(metadata.id);
            UpdateButton();
        }

        public void DeleteMap()
        {
            ShopManager.Instance.DeleteMap(metadata.id);
            UpdateButton();
        }
    }
}