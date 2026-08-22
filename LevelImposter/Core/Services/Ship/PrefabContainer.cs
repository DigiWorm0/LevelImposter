using UnityEngine;

namespace LevelImposter.Core.Services.Ship;

public class PrefabContainer
{
    private Transform? _container;

    /// <summary>
    ///     Provides an inactive container to temporarily create/instantiate prefabs to.
    ///     Since the GameObject is inactive, `Awake()` is not called on any child components
    ///     until their parent gets instantiated.
    /// </summary>
    public Transform Container => GetContainer();

    private Transform GetContainer()
    {
        if (_container != null)
            return _container;

        var container = new GameObject("LI-InactivePrefabs");
        _container = container.transform;
        container.SetActive(false);
        return _container;
    }
}