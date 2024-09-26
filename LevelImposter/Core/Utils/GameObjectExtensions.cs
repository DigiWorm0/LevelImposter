using System;
using UnityEngine;

namespace LevelImposter.Core;

public static class GameObjectExtensions
{
    /// <summary>
    ///     <c>MapObjectData</c> is appended to GameObjects that are built from <c>LIElement</c>s.
    ///     This pulls that object data or throws an Exception if the object was not an <c>LIElement</c>.
    /// </summary>
    /// <param name="gameObject">GameObject to pull data from</param>
    /// <returns>The cooresponding map object data</returns>
    /// <exception cref="Exception">If the object was not an <c>LIElement</c></exception>
    public static MapObjectData GetLIData(this GameObject gameObject)
    {
        var mapObjectData = gameObject.GetComponent<MapObjectData>();
        if (mapObjectData == null)
            throw new Exception($"{gameObject} is missing LI data");
        return mapObjectData;
    }

    /// <summary>
    ///     Equivelent of <c>GameObject.GetComponent</c> but throws an exception if the component is null or missing.
    /// </summary>
    /// <typeparam name="T">Type of component to get</typeparam>
    /// <param name="gameObject">GameObject to search</param>
    /// <returns>Cooresponding component, never null.</returns>
    /// <exception cref="Exception">If the component is null or missing</exception>
    public static T GetComponentOrThrow<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            throw new Exception($"{gameObject} is missing {typeof(T).FullName}");
        return component;
    }

    /// <summary>
    ///     Recursively sets the layer of a GameObject and all of its children.
    /// </summary>
    /// <param name="gameObject">Parent GameObject</param>
    /// <param name="layer">Layer to set to</param>
    public static void SetLayerOfChildren(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        for (var i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetLayerOfChildren(layer);
    }
}