using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LevelImposter.Shop
{
    public static class ShopBuilder
    {
        public static void OnLoad()
        {
            RemoveChildren();
            BuildShop();
        }

        private static void RemoveChildren()
        {
            GameObject controller = GameObject.Find("HowToPlayController");
            controller.transform.FindChild("IntroScene").gameObject.active = false;
            controller.transform.FindChild("RightArrow").gameObject.active = false;
            controller.transform.FindChild("Dots").gameObject.active = false;
        }

        private static void BuildShop()
        {
            // Grab Assets
            Transform ctrlScreen = GameObject.Find("ControllerDisconnectScreen").transform;
            Sprite buttonBackground = ctrlScreen.FindChild("ContinueButton").FindChild("Background").GetComponent<SpriteRenderer>().sprite;
            Sprite contentBackground = ctrlScreen.FindChild("Background").GetComponent<SpriteRenderer>().sprite;

            // Container
            GameObject container = new GameObject("ShopContainer");
            container.transform.position = new Vector3(0, 0, 0);
            SpriteRenderer containerBg = container.AddComponent<SpriteRenderer>();
            containerBg.sprite = contentBackground;
            containerBg.drawMode = SpriteDrawMode.Sliced;
            containerBg.size = new Vector2(7, 5);
            ShopManager shopManager = container.AddComponent<ShopManager>();
            ScrollRect scroller = container.AddComponent<ScrollRect>();
            scroller.horizontal = false;
            scroller.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.parent = container.transform;
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0, 0);
            viewportRect.anchorMax = new Vector2(1, 1);
            viewportRect.pivot = new Vector2(0.5f, 0.5f);
            viewportRect.sizeDelta = new Vector2(0, 0);
            viewport.AddComponent<Mask>();

            // Content
            GameObject content = new GameObject("Content");
            content.transform.parent = viewport.transform;
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(0, 0);
            GridLayoutGroup contentGrid = content.AddComponent<GridLayoutGroup>();
            contentGrid.cellSize = new Vector2(5.0f, 5.0f);
            contentGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            contentGrid.constraintCount = 1;
            ContentSizeFitter contentSize = content.AddComponent<ContentSizeFitter>();
            contentSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject btnPrefab = BuildButtonPrefab(content, buttonBackground);
            shopManager.mapButtonPrefab = btnPrefab;
            shopManager.mapButtonParent = content.transform;
            scroller.viewport = viewport.GetComponent<RectTransform>();
            scroller.content = content.GetComponent<RectTransform>();
        }

        private static GameObject BuildButtonPrefab(GameObject parent, Sprite background)
        {
            GameObject btnPrefab = new GameObject("MapButton");
            btnPrefab.transform.SetParent(parent.transform);
            RectTransform btnRect = btnPrefab.AddComponent<RectTransform>();
            btnRect.drivenProperties = DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.AnchoredPositionY;
            SpriteRenderer btnRenderer = btnPrefab.AddComponent<SpriteRenderer>();
            btnRenderer.sprite = background;
            MapButton btnMapButton = btnPrefab.AddComponent<MapButton>();

            // Button Title
            GameObject btnTitle = new GameObject("Title");
            btnTitle.transform.SetParent(btnPrefab.transform);
            btnTitle.transform.localPosition = new Vector3(0, 0.4f, 0);
            RectTransform btnTitleRect = btnTitle.AddComponent<RectTransform>();
            btnTitleRect.sizeDelta = new Vector2(6, 0.5f);
            TextMeshPro btnTitleText = btnTitle.AddComponent<TextMeshPro>();
            btnTitleText.fontSize = 3.0f;
            btnTitleText.SetText("Ultimate Map v1.0");

            // Button Author
            GameObject btnAuthor = new GameObject("Author");
            btnAuthor.transform.SetParent(btnPrefab.transform);
            btnAuthor.transform.localPosition = new Vector3(0, 0, 0);
            RectTransform btnAuthorRect = btnAuthor.AddComponent<RectTransform>();
            btnAuthorRect.sizeDelta = new Vector2(6, 0.5f);
            TextMeshPro btnAuthorText = btnAuthor.AddComponent<TextMeshPro>();
            btnAuthorText.fontSize = 2.0f;
            btnAuthorText.SetText("DigiWorm");

            // Button Description
            GameObject btnDescription = new GameObject("Description");
            btnDescription.transform.SetParent(btnPrefab.transform);
            btnDescription.transform.localPosition = new Vector3(0, -0.3f, 0);
            RectTransform btnDescriptionRect = btnDescription.AddComponent<RectTransform>();
            btnDescriptionRect.sizeDelta = new Vector2(6, 0.5f);
            TextMeshPro btnDescriptionText = btnDescription.AddComponent<TextMeshPro>();
            btnDescriptionText.fontSize = 2.0f;
            btnDescriptionText.color = new Color(0.5f, 0.5f, 0.5f);
            btnDescriptionText.overflowMode = TextOverflowModes.Ellipsis;
            btnDescriptionText.SetText("A map made for the Ultimate Game Jam\nwow such map\nlorem ipsum dolor sit amet");

            // Download Count
            GameObject btnDownloadCount = new GameObject("DownloadCount");
            btnDownloadCount.transform.SetParent(btnPrefab.transform);
            btnDownloadCount.transform.localPosition = new Vector3(2.0f, -0, 0);
            RectTransform btnDownloadCountRect = btnDownloadCount.AddComponent<RectTransform>();
            btnDownloadCountRect.sizeDelta = new Vector2(0.5f, 0.5f);
            TextMeshPro btnDownloadCountText = btnDownloadCount.AddComponent<TextMeshPro>();
            btnDownloadCountText.fontSize = 2.0f;
            btnDownloadCountText.SetText("0");

            // Delete Button
            GameObject deleteBtn = new GameObject("DeleteButton");
            deleteBtn.transform.SetParent(btnPrefab.transform);
            deleteBtn.transform.localPosition = new Vector3(2.5f, 0, -1.0f);
            SpriteRenderer deleteBtnRenderer = deleteBtn.AddComponent<SpriteRenderer>();
            deleteBtnRenderer.sprite = background;
            deleteBtnRenderer.drawMode = SpriteDrawMode.Sliced;
            deleteBtnRenderer.size = new Vector2(0.5f, 0.5f);
            BoxCollider2D deleteBtnCollider = deleteBtn.AddComponent<BoxCollider2D>();
            deleteBtnCollider.size = new Vector2(0.5f, 0.5f);
            PassiveButton deleteBtnPassive = deleteBtn.AddComponent<PassiveButton>();

            btnPrefab.SetActive(false);
            return btnPrefab;
        }
    }
}
