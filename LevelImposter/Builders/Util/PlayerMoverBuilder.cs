using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class PlayerMoverBuilder : IElemBuilder
{
    private uint _playerMoverCounter = 1;

    public void OnPreBuild()
    {
        _playerMoverCounter = 1;
    }

    public void OnBuild(LIElement elem, GameObject obj)
    {
        if (elem.type != "util-playermover")
            return;

        // Colliders
        Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Add Component
        var playerMover = obj.AddComponent<LIPlayerMover>();
        playerMover.SetObjectID(_playerMoverCounter++);
    }
}