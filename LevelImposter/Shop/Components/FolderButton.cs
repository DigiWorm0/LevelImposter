using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class FolderButton : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private PassiveButton button;

        private bool isHovering = false;

        public void Init()
        {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            button = gameObject.AddComponent<PassiveButton>();

            button.OnMouseOver = new UnityEvent();
            button.OnMouseOut = new UnityEvent();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((Action)OnClick);
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

        public void OnClick()
        {
            Process.Start("explorer.exe", MapLoader.GetDir());
        }
    }
}