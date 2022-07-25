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
        public static Filter currentFilter;
        public static List<FilterButton> filterButtons = new List<FilterButton>();

        private Filter filter;
        private SpriteRenderer spriteRenderer;
        private PassiveButton button;
        private bool isHovering = false;
        private bool isSelected
        {
            get { return filter == currentFilter; }
        }

        public void OnDestory()
        {
            filterButtons.Clear();
        }

        public void SetFilter(Filter newFilter)
        {
            filter = newFilter;
            spriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
            button = GetComponent<PassiveButton>();

            filterButtons.Add(this);

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOut = new UnityEvent();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((Action)UpdateFilter);
            button.OnMouseOut.AddListener((Action)OnMouseOut);
            button.OnMouseOver.AddListener((Action)OnMouseOver);
        }

        public void UpdateButton()
        {   
            if (isSelected)
            {
                spriteRenderer.color = Color.red;
            }
            else
            {
                if (isHovering)
                    spriteRenderer.color = Color.green;
                else
                    spriteRenderer.color = Color.white;
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

        public void UpdateFilter()
        {
            currentFilter = filter;
            switch (currentFilter)
            {
                case Filter.Public:
                    ShopManager.Instance.ListPublicMaps();
                    break;
                case Filter.Downloaded:
                    ShopManager.Instance.ListDownloadedMaps();
                    break;
            }
            foreach (FilterButton btn in filterButtons)
                btn.UpdateButton();
        }

        public enum Filter { 
            Public,
            Downloaded,
        }
    }
}