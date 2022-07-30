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
        public static List<FilterButton> filterButtons = new List<FilterButton>();
        private MapFilter filter;
        private SpriteRenderer spriteRenderer;
        private PassiveButton button;
        private bool isHovering = false;
        private bool isSelected
        {
            get { return filter == ShopManager.Instance.currentFilter; }
        }

        public void Init(MapFilter newFilter)
        {
            filter = newFilter;
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            button = gameObject.AddComponent<PassiveButton>();

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOut = new UnityEvent();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((Action)OnClick);
            button.OnMouseOut.AddListener((Action)OnMouseOut);
            button.OnMouseOver.AddListener((Action)OnMouseOver);

            filterButtons.Add(this);
            UpdateButton();
        }

        public void OnDestroy()
        {
            filterButtons.Remove(this);
        }

        public void UpdateButton()
        {   
            if (isHovering || isSelected)
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

        public void OnClick()
        {
            switch (filter)
            {
                case MapFilter.Recent:
                    ShopManager.Instance.ListRecentMaps();
                    break;
                case MapFilter.Downloaded:
                    ShopManager.Instance.ListDownloadedMaps();
                    break;
                case MapFilter.Verified:
                    ShopManager.Instance.ListVerifiedMaps();
                    break;
            }

            foreach (FilterButton btn in filterButtons)
                btn.UpdateButton();
        }
    }
}