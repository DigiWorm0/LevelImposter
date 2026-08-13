using System;
using System.Collections;
using LevelImposter.Shop.Components;
using Reactor.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Shop.Utils;

public record MapBannerPoolItem(MapBanner MapBanner)
{
    public IEnumerator? TransitionCoroutine;
}

public class MapBannerPool : ObjectPool<MapBannerPoolItem>
{
    private Transform? _container;
    private MapBanner? _prefab;

    public void Initialize(
        MapBanner prefab,
        Transform parent,
        int capacity
    )
    {
        _prefab = prefab;

        _container = new GameObject("MapBannerPool").transform;
        _container.parent = parent;

        AppendCapacity(capacity);
    }

    protected override void OnPoolItemCreated(MapBannerPoolItem item)
    {
        item.MapBanner.gameObject.SetActive(true);
    }

    protected override void OnPoolItemDestroyed(MapBannerPoolItem item)
    {
        item.MapBanner.gameObject.SetActive(false);

        // Stop Animations
        if (item.TransitionCoroutine == null) return;
        Coroutines.Stop(item.TransitionCoroutine);
        item.TransitionCoroutine = null;
    }

    protected override MapBannerPoolItem InitializePoolItem()
    {
        if (_prefab == null)
            throw new NullReferenceException(
                "MapBannerPool prefab is null. Please set the prefab before initializing the pool.");

        var mapBanner = Object.Instantiate(_prefab, _container);
        mapBanner.gameObject.SetActive(false);

        return new MapBannerPoolItem(mapBanner);
    }
}