using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace LevelImposter.Core.Utils;

public static class ArrayExtensions
{
    /// <summary>
    ///     Adds an element to an Il2CppReferenceArray by creating a copy and appending it to the end.
    ///     The array is not modified in-place.
    ///     Instead, a new array is returned.
    /// </summary>
    /// <param name="arr">The original array.</param>
    /// <param name="value">The value to add.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new Il2CppReferenceArray containing the original elements and the new value.</returns>
    public static Il2CppReferenceArray<T> Add<T>(this Il2CppReferenceArray<T> arr, T value)
        where T : Il2CppObjectBase?
    {
        List<T> list = [..arr, value];
        return list.ToArray();
    }

    /// <summary>
    ///     Adds an element to an Il2CppStringArray by creating a copy and appending it to the end.
    ///     The array is not modified in-place.
    ///     Instead, a new array is returned.
    /// </summary>
    /// <param name="arr">The original array.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>A new Il2CppStringArray containing the original elements and the new value.</returns>
    public static Il2CppStringArray Add(this Il2CppStringArray arr, string value)
    {
        List<string> list = [..arr, value];
        return list.ToArray();
    }

    /// <summary>
    ///     Adds an element to an Il2CppStructArray by creating a copy and appending it to the end.
    ///     The array is not modified in-place.
    ///     Instead, a new array is returned.
    /// </summary>
    /// <param name="arr">The original array.</param>
    /// <param name="value">The value to add.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new Il2CppStructArray containing the original elements and the new value.</returns>
    public static Il2CppStructArray<T> Add<T>(this Il2CppStructArray<T> arr, T value)
        where T : unmanaged
    {
        List<T> list = [..arr, value];
        return list.ToArray();
    }

    public static Il2CppReferenceArray<T> Remove<T>(this Il2CppReferenceArray<T> arr, int index)
        where T : Il2CppObjectBase?
    {
        List<T> list = new(arr);
        list.RemoveAt(index);
        return list.ToArray();
    }

    /// <summary>
    ///     Shuffles the elements in an Il2CppStructArray by creating a copy and randomizing it.
    ///     The array is not modified in-place.
    ///     Instead, a new array is returned.
    /// </summary>
    /// <param name="arr">The original array.</param>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <returns>A new Il2CppStructArray containing the original elements in a randomized order.</returns>
    public static Il2CppStructArray<T> Shuffle<T>(this Il2CppStructArray<T> arr)
        where T : unmanaged
    {
        List<T> remainingElements = new(arr);
        List<T> newArray = [];
        while (remainingElements.Count > 0)
        {
            var index = Random.Range(0, remainingElements.Count);
            newArray.Add(remainingElements[index]);
            remainingElements.RemoveAt(index);
        }

        return newArray.ToArray();
    }
}