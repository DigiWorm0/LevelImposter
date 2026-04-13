using System;
using LevelImposter.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Shop;

public static class ShopBuilder
{
    private static GameObject? _mapShopPrefab;
    private static string MapShopPrefabName => GameState.IsMobile ? "Shop-Mobile" : "Shop";

    private static GameObject GetShopPrefab()
    {
        if (_mapShopPrefab == null)
            _mapShopPrefab = MapUtils.LoadResourceFromAssetBundle<GameObject>(MapShopPrefabName);
        if (_mapShopPrefab == null)
            throw new Exception("The shop asset bundle was not found in assembly");
        return _mapShopPrefab;
    }

    public static GameObject Build()
    {
        var mapShop = Object.Instantiate(GetShopPrefab(), Camera.main?.transform, false);
        mapShop.transform.localPosition = new Vector3(0, 0, -990.0f);
        return mapShop;
    }
}