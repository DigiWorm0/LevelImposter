using TMPro;
using UnityEngine;
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
            Sprite contentBackground = ctrlScreen.FindChild("Background").GetComponent<SpriteRenderer>().sprite;

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
            descText.SetText("Once upon a time a thing was a thing that did a thing");

            mapContainer.SetActive(false);
            return mapContainer;
        }
    }
}
