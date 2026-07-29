using System;
using LevelImposter.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Lobby;

/// <summary>
///     Stores and provides access to the lobby dropship as a prefab.
/// </summary>
/// <note>
///     This is seperate from AssetDB since the lobby is stored in the
///     "OnlineGame" scene rather than its own AssetReference.
///     In addition, LobbyBehaviour is not a ShipStatus which certainly doesn't help.
/// </note>
public static class LobbyDropshipPrefab
{
    private static LobbyBehaviour? _prefab => GameStartManager.Instance?.LobbyPrefab ?? null;

    /// <summary>
    ///     Instantiates the children of the lobby dropship onto the transform.
    /// </summary>
    /// <param name="transform">Optional parent transform</param>
    /// <exception cref="System.Exception">If the prefab is not loaded</exception>
    public static void Instantiate(Transform? transform = null)
    {
        if (_prefab == null)
            throw new Exception("Lobby Dropship Prefab not loaded yet!");

        for (var i = 0; i < _prefab.transform.childCount; i++)
        {
            var prefabChild = _prefab.transform.GetChild(i);
            LILogger.Debug($"Instantiating {prefabChild.name} ({i}/{_prefab.transform.childCount})");
            Object.Instantiate(prefabChild.gameObject, transform);
        }
    }

    /// <summary>
    ///     Gets a GameObject from the lobby dropship prefab
    ///     by its transform path
    /// </summary>
    /// <param name="path">The transform path to the target object</param>
    /// <returns>The target GameObject</returns>
    /// <exception cref="System.Exception">If the prefab is not loaded or the path is not found</exception>
    public static GameObject GetObjectFromPrefab(string path)
    {
        // Check if prefab is loaded
        if (_prefab == null)
            throw new Exception("Lobby Dropship Prefab not loaded yet!");

        // Find target object
        var targetTransform = _prefab.transform.Find(path);
        if (targetTransform == null)
            throw new Exception($"Path '{path}' not found in Lobby Dropship Prefab!");

        // Return target GameObject
        return targetTransform.gameObject;
    }
}