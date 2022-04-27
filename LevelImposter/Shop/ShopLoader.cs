using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Shop
{
    public static class ShopLoader
    {
        public static void OnLoad()
        {
            RemoveChildren();
            AddShopAssets();
        }

        private static void RemoveChildren()
        {
            GameObject controller = GameObject.Find("HowToPlayController");
            controller.transform.FindChild("IntroScene").gameObject.active = false;
            controller.transform.FindChild("RightArrow").gameObject.active = false;
            controller.transform.FindChild("Dots").gameObject.active = false;
        }

        private static void AddShopAssets()
        {
            byte[] mapShopData = Properties.Resources.mapshop;
            AssetBundle mapShopBundle = AssetBundle.LoadFromMemory(mapShopData);
            UnityEngine.Object mapShop = mapShopBundle.LoadAsset("assets/prefabs/canvas.prefab");
            GameObject.Instantiate(mapShop);
            
            // To Be Implemented Text
            GameObject toBeImplemented = new GameObject("ToBeImplemented");
            toBeImplemented.transform.position = new Vector3(7.5f, -2.0f, 1.0f);

            TMPro.TextMeshPro textComponent = toBeImplemented.AddComponent<TMPro.TextMeshPro>();
            textComponent.text = "To Be Implemented";
            textComponent.fontSize = 6;
        }
    }
}
