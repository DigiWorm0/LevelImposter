using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Database of all map objects
/// </summary>
public class MapObjectDB
{
    private readonly Dictionary<Guid, GameObject> _guidToGameObject = new();
    private readonly Dictionary<int, LIElement> _instanceIDToElement = new();

    /// <summary>
    ///     Adds a constructed map element to the database
    /// </summary>
    /// <param name="element">The map element to add</param>
    /// <param name="gameObject">The GameObject associated with the map element</param>
    public void AddObject(
        LIElement element,
        GameObject gameObject)
    {
        _guidToGameObject[element.id] = gameObject;
        _instanceIDToElement[gameObject.GetInstanceID()] = element;
    }

    /// <summary>
    ///     Gets an object from the database
    /// </summary>
    /// <param name="guid">Element ID</param>
    /// <returns>The cooresponding GameObject or null if it wasn't found</returns>
    public GameObject? GetObject(Guid guid)
    {
        return _guidToGameObject.GetValueOrDefault(guid);
    }
    
    /// <summary>
    /// Gets an element from the database by the looking up the GameObject's instance ID.
    /// </summary>
    /// <param name="gameObject">The GameObject to look up</param>
    /// <returns>The corresponding LIElement or null if not found</returns>
    public LIElement? GetElement(GameObject gameObject)
    {
        var instanceID = gameObject.GetInstanceID();
        return _instanceIDToElement.GetValueOrDefault(instanceID);
    }
    
    /// <summary>
    ///    Gets an element from the database by the looking up the GameObject's instance ID.
    /// </summary>
    /// <param name="gameObject">The GameObject to look up</param>
    /// <returns>The corresponding LIElement or null if not found</returns>
    public static LIElement? Get(GameObject gameObject)
    {
        // TODO: Remove dependency on LIShipStatus
        return LIShipStatus.MapObjectDB.GetElement(gameObject);
    }
}