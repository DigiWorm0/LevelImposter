using System;
using UnityEngine;

namespace LevelImposter.Core;

public static class GameObjectExtensions
{
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
    
    /// <summary>
    /// Gets a component of type T from the GameObject.
    /// If the component does not exist, it is added.
    /// </summary>
    /// <param name="gameObject">The GameObject to get or add the component to.</param>
    /// <typeparam name="T">The type of component to get or add.</typeparam>
    /// <returns>The existing or newly added component of type T.</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }
}