using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Components;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class PlayerMoverBuilder
{
    private static uint _playerMoverCounter = 1;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _playerMoverCounter = 1;
    }

    [ElementBuilder(ElementTypes = ["util-playermover"])]
    public static void Build(GameObject gameObject)
    {
        // Colliders
        Collider2D[] colliders = gameObject.GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders)
            collider.isTrigger = true;

        // Add Component
        var playerMover = gameObject.AddComponent<LIPlayerMover>();
        playerMover.SetObjectID(_playerMoverCounter++);
    }
}