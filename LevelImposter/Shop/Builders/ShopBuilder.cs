using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LevelImposter.Core;
using LevelImposter.DB;

namespace LevelImposter.Shop
{
    public static class ShopBuilder
    {
        private static GameObject _mapShopPrefab = null;

        public static void OnCustomizationMenu()
        {
            bool isInLobby = LobbyBehaviour.Instance != null;
            if (isInLobby)
                return;

            // Shop Spawner
            GameObject shopSpawner = new("Shop Spawner");
            shopSpawner.AddComponent<ShopSpawner>();
        }

        private static GameObject GetShopPrefab()
        {
            if (_mapShopPrefab == null)
                _mapShopPrefab = MapUtils.LoadAssetBundle("shop");
            if (_mapShopPrefab == null)
                throw new System.Exception("The \"shop\" asset bundle was not found in assembly");
            return _mapShopPrefab;
        }

        public static GameObject Build()
        {
            GameObject mapShop = Object.Instantiate(GetShopPrefab());

            // Scroll
            ShopManager shopMgr = mapShop.AddComponent<ShopManager>();
            shopMgr.ShopParent = mapShop.transform.FindChild("Canvas").FindChild("Scroll").FindChild("Viewport").FindChild("Content");
            shopMgr.MapBannerPrefab = shopMgr.ShopParent.FindChild("MapBanner").gameObject.AddComponent<MapBanner>();

            // Nav Buttons
            ShopButtons shopBtns = mapShop.AddComponent<ShopButtons>();

            // Shop Banner
            MapBanner bannerPrefab = shopMgr.MapBannerPrefab;
            bannerPrefab.transform.FindChild("LoadOverlay").FindChild("LoadingSpinner").gameObject.AddComponent<Spinner>();

            // Close Button
            Button closeButton = mapShop.transform.FindChild("Canvas").FindChild("CloseBtn").GetComponent<Button>();
            closeButton.onClick.AddListener((System.Action)ShopManager.CloseShop);
            shopMgr.CloseButton = closeButton;

            // Star Field
            Transform starField = mapShop.transform.FindChild("Star Field");
            StarGen starGen = starField.gameObject.AddComponent<StarGen>();
            starGen.Length = 12;
            starGen.Width = 6;

            // Star Renderer
            MeshRenderer starRenderer = starField.gameObject.GetComponent<MeshRenderer>();
            var prefabShip = AssetDB.GetObject("ss-skeld");
            if (prefabShip == null)
                return null;
            // TODO: Move Starfield to ObjectDB
            starRenderer.material = prefabShip.transform.FindChild("starfield").gameObject.GetComponent<MeshRenderer>().material;

            // Background
            GameObject background = mapShop.transform.FindChild("Background").gameObject;
            PassiveButton clickMask = mapShop.AddComponent<PassiveButton>();
            clickMask.Colliders = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Collider2D>(new Collider2D[] {
                background.GetComponent<BoxCollider2D>()
            });

            return mapShop;
        }
    }
}
