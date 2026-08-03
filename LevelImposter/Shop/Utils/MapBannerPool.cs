using System;
using System.Collections;
using LevelImposter.Shop.Components;
using Reactor.Utilities;
using Object = UnityEngine.Object;

namespace LevelImposter.Shop.Utils;

public class MapBannerPoolItem
{
    public MapBanner MapBanner;
    public IEnumerator? TransitionCoroutine;
}

public class MapBannerPool : ObjectPool<MapBannerPoolItem>
{
    private MapBanner? _prefab;

    public void Initialize(
        MapBanner prefab,
        int capacity
    )
    {
        _prefab = prefab;
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

        var mapBanner = Object.Instantiate(_prefab);
        mapBanner.gameObject.SetActive(false);

        return new MapBannerPoolItem
        {
            MapBanner = mapBanner
        };
    }
}