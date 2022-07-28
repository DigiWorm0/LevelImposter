using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class MapBanner : MonoBehaviour
    {
        public bool isDownloading = false;
        public bool isDownloaded
        {
            get { return MapLoader.Exists(metadata.id); }
        }

        private TMPro.TMP_Text titleText;
        private TMPro.TMP_Text descriptionText;
        private TMPro.TMP_Text authorText;

        private GameObject deleteBtn;
        private GameObject downloadBtn;
        private GameObject launchBtn;

        private LIMetadata metadata;

        private List<MapBannerButton> buttons = new List<MapBannerButton>();

        public void Init(LIMetadata metadata)
        {
            this.metadata = metadata;
            gameObject.SetActive(true);

            titleText = transform.Find("Title").GetComponent<TMPro.TMP_Text>();
            descriptionText = transform.Find("Description").GetComponent<TMPro.TMP_Text>();
            authorText = transform.Find("Author").GetComponent<TMPro.TMP_Text>();

            deleteBtn = transform.Find("DeleteBtn").gameObject;
            deleteBtn.AddComponent<MapBannerButton>().Init(MapButtonFunction.Delete);
            buttons.Add(deleteBtn.GetComponent<MapBannerButton>());

            launchBtn = transform.Find("RunBtn").gameObject;
            launchBtn.AddComponent<MapBannerButton>().Init(MapButtonFunction.Launch);
            buttons.Add(launchBtn.GetComponent<MapBannerButton>());

            downloadBtn = transform.Find("DownloadBtn").gameObject;
            downloadBtn.AddComponent<MapBannerButton>().Init(MapButtonFunction.Download);
            buttons.Add(downloadBtn.GetComponent<MapBannerButton>());

            titleText.text = metadata.name;
            if (string.IsNullOrEmpty(metadata.authorID))
            {
                authorText.fontStyle = TMPro.FontStyles.Italic;
                authorText.text = "(Freeplay Only)";
                descriptionText.text = metadata.id + ".lim";
            }
            else
            {
                authorText.text = "By: " + metadata.authorName;
                descriptionText.text = metadata.description;
            }
        }

        public void DownloadMap()
        {
            isDownloading = true;
            Guid mapID = Guid.Empty;
            Guid.TryParse(metadata.id, out mapID);
            if (mapID != Guid.Empty)
                ShopManager.Instance.DownloadMap(mapID, OnDownloadFinish);
            UpdateAllButtons();
        }

        public void OnDownloadFinish()
        {
            isDownloading = false;
            MapLoader.LoadMap(metadata.id);
            UpdateAllButtons();
        }

        public void DeleteMap()
        {
            ShopManager.Instance.DeleteMap(metadata.id);
            UpdateAllButtons();
        }

        public void LaunchMap()
        {
            ShopManager.Instance.LaunchMap(metadata.id);
        }

        private void UpdateAllButtons()
        {
            foreach (MapBannerButton btn in buttons)
            {
                btn.UpdateButton();
            }
        }
    }
}