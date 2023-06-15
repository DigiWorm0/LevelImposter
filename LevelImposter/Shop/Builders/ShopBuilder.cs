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
        private static GameObject? _mapShopPrefab = null;

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
            mapShop.transform.position = new Vector3(0, 0, -301.0f);
            return mapShop;
        }
    }
}
