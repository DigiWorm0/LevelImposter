using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class FilterButton : MonoBehaviour
    {
        private Filter filter;
        private SpriteRenderer spriteRenderer;
        private PassiveButton button;
        private bool isHovering = false;

        public void SetFilter(Filter newFilter)
        {
            filter = newFilter;
            spriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
            button = GetComponent<PassiveButton>();

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOut = new UnityEvent();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((Action)UpdateFilter);
            button.OnMouseOut.AddListener((Action)OnMouseOut);
            button.OnMouseOver.AddListener((Action)OnMouseOver);
        }

        public void UpdateButton()
        {   
            if (isHovering)
                spriteRenderer.color = Color.green;
            else
                spriteRenderer.color = Color.white;
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

        public void UpdateFilter()
        {
            switch (filter)
            {
                case Filter.Public:
                    ShopManager.Instance.ListPublicMaps();
                    break;
                case Filter.Downloaded:
                    ShopManager.Instance.ListDownloadedMaps();
                    break;
            }
        }

        public enum Filter { 
            Public,
            Downloaded,
        }
    }
}