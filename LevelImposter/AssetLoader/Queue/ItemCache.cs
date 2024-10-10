using System.Collections.Generic;

namespace LevelImposter.AssetLoader;

public class ItemCache<T>
{
    private readonly Dictionary<string, T> _cachedItems = new();
    public int Count => _cachedItems.Count;

    public void Add(string id, T asset)
    {
        _cachedItems[id] = asset;
    }

    public T? Get(string id)
    {
        return _cachedItems.GetValueOrDefault(id);
    }

    public void Clear()
    {
        _cachedItems.Clear();
    }
}