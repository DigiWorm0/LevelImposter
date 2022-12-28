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

        public static void OnLoad()
        {
            // Remove Children
            GameObject controller = GameObject.Find("HowToPlayController");
            controller.transform.FindChild("HnS Scenes").gameObject.active = false;
            controller.transform.FindChild("ClassicScenes").gameObject.active = false;
            controller.transform.FindChild("Game modes").gameObject.active = false;

            // Shop Spawner
            GameObject shopSpawner = new("Shop Spawner");
            shopSpawner.AddComponent<ShopSpawner>();
        }

        private static GameObject GetShopPrefab()
        {
            if (_mapShopPrefab == null)
                _mapShopPrefab = MapUtils.LoadAssetBundle("shop");
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
            Transform btnsParent = mapShop.transform.FindChild("Canvas").FindChild("Shop Buttons");
            shopBtns.DownloadedButton = btnsParent.FindChild("DownloadedBtn").GetComponent<Button>();
            shopBtns.TopButton = btnsParent.FindChild("TopBtn").GetComponent<Button>();
            shopBtns.RecentButton = btnsParent.FindChild("RecentBtn").GetComponent<Button>();
            shopBtns.FeaturedButton = btnsParent.FindChild("FeaturedBtn").GetComponent<Button>();
            shopBtns.FolderButton = btnsParent.FindChild("FolderBtn").GetComponent<Button>();

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
            Transform skeld = AssetDB.Ships["ss-skeld"].ShipStatus.transform;
            starRenderer.material = skeld.FindChild("starfield").gameObject.GetComponent<MeshRenderer>().material;

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
