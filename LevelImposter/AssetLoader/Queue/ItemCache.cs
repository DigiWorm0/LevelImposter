using System.Collections.Generic;
using LevelImposter.Core;

namespace LevelImposter.AssetLoader;

public class ItemCache<T> where T : ICachable
{
    private readonly Dictionary<string, T> _cachedItems = new();
    public int Count => _cachedItems.Count;

    public void Add(string id, T asset)
    {
        _cachedItems[id] = asset;
    }

    public T? Get(string id)
    {
        if (!_cachedItems.TryGetValue(id, out var item))
            return default;
        
        if (!item.IsExpired)
            return item;
        
        _cachedItems.Remove(id);
        return default;
    }

    public void Clear()
    {
        _cachedItems.Clear();
    }
}