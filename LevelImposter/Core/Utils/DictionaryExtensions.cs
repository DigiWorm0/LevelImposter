using System.Collections.Generic;

namespace LevelImposter.Core.Utils;

public static class DictionaryExtensions
{
    public static TValue? GetValueOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        where TValue : class
    {
        return dict.TryGetValue(key, out var value) ? value : null;
    }

    public static TValue? GetValueOrNullStruct<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        where TValue : struct // Safely wraps value types into a Nullable<T>
    {
        return dict.TryGetValue(key, out var value) ? value : null;
    }
}