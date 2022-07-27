using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
            GameObject starField = GameObject.Find("starfield");
            //starField.SetActive(false);
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

            // Container
            GameObject shopContainer = new GameObject("ShopContainer");
            shopContainer.transform.position = new Vector3(1.4f, 0, 0);
            Scroller scroller = shopContainer.AddComponent<Scroller>();
            scroller.allowY = true;
            scroller.ContentYBounds = new FloatRange(-1.8f, -1.8f);
            scroller.ContentXBounds = new FloatRange(0, 0);
            scroller.ScrollbarXBounds = new FloatRange(0, 0);
            scroller.ScrollbarYBounds = new FloatRange(0, 0);
            ShopManager shopManager = shopContainer.AddComponent<ShopManager>();

            // Background
            GameObject shopBackground = new GameObject("Background");
            shopBackground.transform.SetParent(shopContainer.transform);
            shopBackground.transform.localPosition = new Vector3(0, 0, 5);
            BoxCollider2D bgCollider = shopBackground.AddComponent<BoxCollider2D>();
            bgCollider.size = new Vector2(7, 7);
            scroller.Colliders = new UnhollowerBaseLib.Il2CppReferenceArray<Collider2D>(1);
            scroller.Colliders[0] = bgCollider;

            // Inner
            GameObject shopInner = new GameObject("Inner");
            shopInner.transform.SetParent(shopContainer.transform);
            shopInner.transform.localPosition = Vector3.zero;
            scroller.Inner = shopInner.transform;

            GameObject button = BuildButtonPrefab(shopInner, buttonBackground);
            shopManager.mapButtonPrefab = button;
            shopManager.mapButtonParent = shopInner.transform;

            BuildSubContainer(buttonBackground);
        }

        private static GameObject BuildButtonPrefab(GameObject parent, Sprite background)
        {
            // Container
            GameObject mapContainer = new GameObject("MapContainer");
            mapContainer.transform.SetParent(parent.transform);
            mapContainer.transform.localPosition = Vector3.zero;
            BoxCollider2D mapCollider = mapContainer.AddComponent<BoxCollider2D>();
            mapCollider.size = new Vector2(6, 1);
            mapCollider.offset = Vector2.zero;
            PassiveButton mapPassiveBtn = mapContainer.AddComponent<PassiveButton>();
            MapButton mapButton = mapContainer.AddComponent<MapButton>();

            // Background
            GameObject mapBackground = new GameObject("Background");
            mapBackground.transform.SetParent(mapContainer.transform);
            mapBackground.transform.localPosition = Vector3.zero;
            SpriteRenderer spriteRenderer = mapBackground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = background;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(6, 1);

            // Title
            GameObject mapTitle = new GameObject("Title");
            mapTitle.transform.SetParent(mapContainer.transform);
            mapTitle.transform.localPosition = new Vector3(0, 0, 0);
            RectTransform titleTransform = mapTitle.AddComponent<RectTransform>();
            titleTransform.sizeDelta = new Vector2(5.5f, 0.8f);
            TextMeshPro titleText = mapTitle.AddComponent<TextMeshPro>();
            titleText.fontSize = 2.5f;
            titleText.raycastTarget = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.SetText("Example Map");

            // Author
            GameObject mapAuthor = new GameObject("Author");
            mapAuthor.transform.SetParent(mapContainer.transform);
            mapAuthor.transform.localPosition = new Vector3(0, 0, 0);
            RectTransform authorTransform = mapAuthor.AddComponent<RectTransform>();
            authorTransform.sizeDelta = new Vector2(5.5f, 0.8f);
            TextMeshPro authorText = mapAuthor.AddComponent<TextMeshPro>();
            authorText.fontSize = 2.0f;
            authorText.alignment = TextAlignmentOptions.Left;
            authorText.raycastTarget = false;
            authorText.SetText("DigiWorm");

            // Description
            GameObject mapDesc = new GameObject("Description");
            mapDesc.transform.SetParent(mapContainer.transform);
            mapDesc.transform.localPosition = new Vector3(0, 0, 0);
            RectTransform descTransform = mapDesc.AddComponent<RectTransform>();
            descTransform.sizeDelta = new Vector2(5.5f, 0.7f);
            TextMeshPro descText = mapDesc.AddComponent<TextMeshPro>();
            descText.fontSize = 1.8f;
            descText.alignment = TextAlignmentOptions.BottomLeft;
            descText.color = new Color(0.5f, 0.5f, 0.5f);
            descText.raycastTarget = false;
            descText.SetText("Once upon a time a thing was a thing that did a thing");

            mapContainer.SetActive(false);
            return mapContainer;
        }

        public static GameObject BuildSubContainer(Sprite background)
        {
            // Sub Container
            GameObject subContainer = new GameObject("ShopSubContainer");
            subContainer.transform.position = new Vector3(-3.2f, 1.4f, 0);

            // Sub Title
            GameObject subTitle = new GameObject("Title");
            subTitle.transform.SetParent(subContainer.transform);
            subTitle.transform.localPosition = new Vector3(0, 0, 0);
            RectTransform subTitleTransform = subTitle.AddComponent<RectTransform>();
            subTitleTransform.sizeDelta = new Vector2(5.0f, 1.0f);
            TextMeshPro subTitleText = subTitle.AddComponent<TextMeshPro>();
            subTitleText.fontSize = 4;
            subTitleText.SetText("\nMaps<size=2> v" + LevelImposter.VERSION);
            subTitleText.alignment = TextAlignmentOptions.Top;

            // Sub Logo
            GameObject subLogo = new GameObject("Logo");
            subLogo.transform.SetParent(subContainer.transform);
            subLogo.transform.localPosition = new Vector3(0, 0.2f);

            SpriteRenderer logoRenderer = subLogo.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, Properties.Resources.logo);
            logoRenderer.sprite = Sprite.Create(
                tex,
                new Rect(0.0f, 0.0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                180.0f
            );

            // Public Filter
            GameObject publicButton = BuildFilterButton(background, "Public");
            publicButton.transform.SetParent(subContainer.transform);
            publicButton.transform.localPosition = new Vector3(0, -2.0f);
            publicButton.AddComponent<FilterButton>().SetFilter(FilterButton.Filter.Public);

            // Download Filter
            GameObject downloadedButton = BuildFilterButton(background, "Downloaded");
            downloadedButton.transform.SetParent(subContainer.transform);
            downloadedButton.transform.localPosition = new Vector3(0, -2.5f);
            downloadedButton.AddComponent<FilterButton>().SetFilter(FilterButton.Filter.Downloaded);

            // Folder Filter
            GameObject folderButton = BuildFilterButton(background, "Open Folder");
            folderButton.transform.SetParent(subContainer.transform);
            folderButton.transform.localPosition = new Vector3(0, -3.5f);
            PassiveButton folderBtn = folderButton.GetComponent<PassiveButton>();
            folderBtn.OnMouseOver = new UnityEvent();
            folderBtn.OnMouseOut = new UnityEvent();
            folderBtn.OnClick.AddListener((System.Action)(() =>
            {
                Process.Start("explorer.exe", MapLoader.GetDir());
            }));

            return subContainer;
        }

        private static GameObject BuildFilterButton(Sprite background, string title)
        {
            // Container
            GameObject filterButton = new GameObject("SubButton-" + title);
            BoxCollider2D filterCollider = filterButton.AddComponent<BoxCollider2D>();
            filterCollider.size = new Vector2(2, 0.4f);
            filterCollider.offset = Vector2.zero;
            PassiveButton filterPassiveBtn = filterButton.AddComponent<PassiveButton>();

            // Background
            GameObject filterBackground = new GameObject("Background");
            filterBackground.transform.SetParent(filterButton.transform);
            filterBackground.transform.localPosition = Vector3.zero;
            SpriteRenderer spriteRenderer = filterBackground.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = background;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(2, 0.4f);

            // Title
            GameObject filterTitle = new GameObject("Title");
            filterTitle.transform.SetParent(filterButton.transform);
            filterTitle.transform.localPosition = Vector3.zero;
            RectTransform titleTransform = filterTitle.AddComponent<RectTransform>();
            titleTransform.sizeDelta = new Vector2(1.5f, 0.4f);
            TextMeshPro titleText = filterTitle.AddComponent<TextMeshPro>();
            titleText.fontSize = 2.0f;
            titleText.raycastTarget = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = title;

            return filterButton;
        }
    }
}
