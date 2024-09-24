using System.Collections.Generic;
using LevelImposter.Core;

namespace LevelImposter.DB;

/// <summary>
///     A miniature DB designed to search and
///     contain various types of objects within Among Us
/// </summary>
/// <typeparam name="T">Type of object to contain</typeparam>
public abstract class SubDB<T>(SerializedAssetDB serializedDB)
{
    private readonly Dictionary<string, T> _data = new();

    protected SerializedAssetDB DB { get; } = serializedDB;

    /// <summary>
    ///     Loads any extra assets into the DB
    /// </summary>
    public virtual void Load()
    {
    }

    /// <summary>
    ///     Loads each ship into the DB
    /// </summary>
    /// <param name="shipStatus">ShipStatus to load</param>
    /// <param name="mapType">MapType of ShipStatus</param>
    public virtual void LoadShip(ShipStatus shipStatus, MapType mapType)
    {
    }

    /// <summary>
    ///     Gets an object from the DB
    /// </summary>
    /// <param name="id">Type ID of the object</param>
    /// <returns>Object or null if not found</returns>
    public T? Get(string id)
    {
        _data.TryGetValue(id, out var result);
        return result;
    }

    /// <summary>
    ///     Adds an object to the DB
    /// </summary>
    /// <param name="id">ID of the object</param>
    /// <param name="obj">Object to add</param>
    protected void Add(string id, T obj)
    {
        _data.Add(id, obj);
    }
}