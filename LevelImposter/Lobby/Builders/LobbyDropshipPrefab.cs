using UnityEngine;

namespace LevelImposter.Lobby;

/// <summary>
/// Stores and provides access to the lobby dropship as a prefab.
/// </summary>
/// <note>
/// This is seperate from AssetDB since the lobby is stored in the
/// "OnlineGame" scene rather than its own AssetReference.
/// In addition, LobbyBehaviour is not a ShipStatus which certainly doesn't help.
/// </note>
public static class LobbyDropshipPrefab
{
    private static GameObject? _prefab;

    /// <summary>
    /// Called when the lobby is loaded to
    /// retrieve and store the dropship prefab
    /// </summary>
    public static void OnLobbyLoad()
    {
        if (_prefab != null)
            return;
        
        // Create a disabled container to hold the prefab instance
        var prefabContainer = new GameObject("LI_DropshipPrefabContainer");
        prefabContainer.SetActive(false);
        
        // Instantiate Dropship prefab
        var dropship = LILobbyBehaviour.GetInstance();
        _prefab = Object.Instantiate(dropship.gameObject, prefabContainer.transform);
    }

    /// <summary>
    /// Instantiates a new instance of the lobby dropship prefab
    /// </summary>
    /// <param name="transform">Optional parent transform</param>
    /// <returns>The instantiated GameObject</returns>
    /// <exception cref="System.Exception">If the prefab is not loaded</exception>
    public static GameObject Instantiate(Transform? transform = null)
    {
        if (_prefab == null)
            throw new System.Exception("Lobby Dropship Prefab not loaded yet!");

        return Object.Instantiate(_prefab, transform);
    }

    /// <summary>
    /// Gets a GameObject from the lobby dropship prefab
    /// by its transform path
    /// </summary>
    /// <param name="path">The transform path to the target object</param>
    /// <returns>The target GameObject</returns>
    /// <exception cref="System.Exception">If the prefab is not loaded or the path is not found</exception>
    public static GameObject GetObjectFromPrefab(string path)
    {
        // Check if prefab is loaded
        if (_prefab == null)
            throw new System.Exception("Lobby Dropship Prefab not loaded yet!");

        // Find target object
        var targetTransform = _prefab.transform.Find(path);
        if (targetTransform == null)
            throw new System.Exception($"Path '{path}' not found in Lobby Dropship Prefab!");

        // Return target GameObject
        return targetTransform.gameObject;
    }
}