using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public static class ShopBuilder
    {
        private static GameObject mapShopPrefab = null;

        public static void OnLoad()
        {
            RemoveChildren();
            BuildShop();
        }

        private static GameObject LoadAssetBundle(string name)
        {
            Stream resourceStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("LevelImposter.Assets." + name);
            using (var ms = new MemoryStream())
            {
                resourceStream.CopyTo(ms);
                byte[] assetData = ms.ToArray();
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(assetData);
                GameObject asset = assetBundle.LoadAsset(name, UnhollowerRuntimeLib.Il2CppType.Of<GameObject>()).Cast<GameObject>();
                assetBundle.Unload(false);
                return asset;
            }
        }

        public static GameObject GetShopPrefab()
        {
            if (mapShopPrefab == null)
                mapShopPrefab = LoadAssetBundle("shop");
            return mapShopPrefab;
        }

        private static void RemoveChildren()
        {
            GameObject controller = GameObject.Find("HowToPlayController");
            controller.transform.FindChild("IntroScene").gameObject.active = false;
            controller.transform.FindChild("RightArrow").gameObject.active = false;
            controller.transform.FindChild("Dots").gameObject.active = false;
        }

        public static GameObject BuildShop()
        {
            GameObject mapShop = Object.Instantiate(GetShopPrefab());

            ShopManager shopMgr = mapShop.AddComponent<ShopManager>();
            shopMgr.shopParent = mapShop.transform.FindChild("Canvas").FindChild("Scroll").FindChild("Viewport").FindChild("Content");
            shopMgr.mapBannerPrefab = shopMgr.shopParent.FindChild("MapBanner").gameObject.AddComponent<MapBanner>();

            ShopButtons shopBtns = mapShop.AddComponent<ShopButtons>();
            Transform btnsParent = mapShop.transform.FindChild("Canvas").FindChild("Shop Buttons");
            shopBtns.downloadedButton = btnsParent.FindChild("DownloadedBtn").GetComponent<Button>();
            shopBtns.topButton = btnsParent.FindChild("TopBtn").GetComponent<Button>();
            shopBtns.recentButton = btnsParent.FindChild("RecentBtn").GetComponent<Button>();
            shopBtns.featuredButton = btnsParent.FindChild("FeaturedBtn").GetComponent<Button>();
            shopBtns.folderButton = btnsParent.FindChild("FolderBtn").GetComponent<Button>();

            MapBanner bannerPrefab = shopMgr.mapBannerPrefab;
            bannerPrefab.transform.FindChild("LoadOverlay").FindChild("LoadingSpinner").gameObject.AddComponent<Spinner>();

            mapShop.transform.FindChild("Canvas").FindChild("CloseBtn").GetComponent<Button>().onClick.AddListener((System.Action)ShopManager.CloseShop);

            return mapShop;
        }
    }
}
