using System;
using UnityEngine;

namespace LevelImposter.Lobby.Builders;

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
    private static LobbyBehaviour? Prefab => GameStartManager.Instance?.LobbyPrefab ?? null;

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
        if (Prefab == null)
            throw new Exception("Lobby Dropship Prefab not loaded yet!");

        // Find target object
        var targetTransform = Prefab.transform.Find(path);
        if (targetTransform == null)
            throw new Exception($"Path '{path}' not found in Lobby Dropship Prefab!");

        // Return target GameObject
        return targetTransform.gameObject;
    }
}