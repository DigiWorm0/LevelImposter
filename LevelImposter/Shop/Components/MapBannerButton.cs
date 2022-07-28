using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class MapBannerButton : MonoBehaviour
    {
        
        private SpriteRenderer spriteRenderer;
        private PassiveButton button;
        private MapBanner banner;
        private MapButtonFunction function;

        private bool isHovering = false;

        public void Init(MapButtonFunction func)
        {
            function = func;
            banner = gameObject.GetComponentInParent<MapBanner>();
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            button = gameObject.AddComponent<PassiveButton>();

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOut = new UnityEvent();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((Action)OnClick);
            button.OnMouseOut.AddListener((Action)OnMouseOut);
            button.OnMouseOver.AddListener((Action)OnMouseOver);

            UpdateButton();
        }

        public void UpdateButton()
        {
            if (isHovering)
                spriteRenderer.color = Color.green;
            else
                spriteRenderer.color = Color.white;

            switch (function)
            {
                case MapButtonFunction.Delete:
                    gameObject.SetActive(banner.isDownloaded);
                    break;
                case MapButtonFunction.Launch:
                    gameObject.SetActive(banner.isDownloaded);
                    break;
                case MapButtonFunction.Download:
                    gameObject.SetActive(!banner.isDownloading && !banner.isDownloaded);
                    break;
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

        public void OnClick()
        {
            
            switch (function)
            {
                case MapButtonFunction.Delete:
                    banner.DeleteMap();
                    break;
                case MapButtonFunction.Launch:
                    banner.LaunchMap();
                    break;
                case MapButtonFunction.Download:
                    banner.DownloadMap();
                    break;
            }
        }
    }
}
