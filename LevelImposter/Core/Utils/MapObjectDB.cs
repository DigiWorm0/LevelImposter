using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Database of all map objects
/// </summary>
public class MapObjectDB
{
    private readonly Dictionary<Guid, GameObject> _mapObjectDB = new();

    /// <summary>
    ///     Adds an object to the database
    /// </summary>
    /// <param name="guid">Element ID</param>
    /// <param name="obj">Cooresponding GameObject</param>
    public void AddObject(Guid guid, GameObject obj)
    {
        _mapObjectDB[guid] = obj;
    }

    /// <summary>
    ///     Gets an object from the database
    /// </summary>
    /// <param name="guid">Element ID</param>
    /// <returns>The cooresponding GameObject or null if it wasn't found</returns>
    public GameObject? GetObject(Guid guid)
    {
        return _mapObjectDB.GetValueOrDefault(guid);
    }

    /// <summary>
    ///     Gets an object from the database
    /// </summary>
    /// <param name="guid">Element ID</param>
    /// <returns>The cooresponding GameObject or null if it wasn't found</returns>
    public static GameObject? Get(Guid guid)
    {
        // Get Ship Status
        var shipStatus = LIShipStatus.GetInstanceOrNull();
        if (shipStatus == null)
        {
            LILogger.Warn("Ship Status is missing");
            return null;
        }

        // Get object
        return shipStatus.MapObjectDB.GetObject(guid);
    }
}