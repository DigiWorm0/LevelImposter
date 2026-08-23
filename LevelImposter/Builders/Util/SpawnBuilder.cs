using System.Collections.Generic;
using LevelImposter.Build;
using LevelImposter.Build.Attributes;
using LevelImposter.Core.Models;
using LevelImposter.Core.Utils;
using UnityEngine;

namespace LevelImposter.Builders.Util;

internal static class SpawnBuilder
{
    private const float DEFAULT_SPAWN_RADIUS = 1.55f;
    private const int DUMMY_SPAWN_COUNT = 15;

    private static Vector2 _fallbackSpawn;
    private static bool _hasInitialSpawn;
    private static bool _hasMeetingSpawn;

    [MapBuilder(Priority = Priority.FIRST)]
    public static void Reset()
    {
        _hasInitialSpawn = false;
        _hasMeetingSpawn = false;
        _fallbackSpawn = Vector2.zero;
    }

    [ElementBuilder(
        Target = MapTarget.Game,
        ElementTypes = ["util-spawn1", "util-spawn2"]
    )]
    public static void Build(ShipStatus shipStatus, LIElement element, GameObject gameObject)
    {
        // Set Spawn Radius
        shipStatus.SpawnRadius = element.properties.range ?? DEFAULT_SPAWN_RADIUS;

        // Set Spawn Point
        Vector2 pos = gameObject.transform.position - new Vector3(0f, 0.3636f, 0f);
        _fallbackSpawn = pos;
        if (element.type == "util-spawn1")
        {
            shipStatus.InitialSpawnCenter = pos;
            _hasInitialSpawn = true;
        }
        else if (element.type == "util-spawn2")
        {
            shipStatus.MeetingSpawnCenter = pos;
            shipStatus.MeetingSpawnCenter2 = pos;
            _hasMeetingSpawn = true;
        }
        else
        {
            LILogger.Warn($"{element.name} has an unknown spawn type");
        }

        // Add Dummy Locations
        var spawnDummies = element.properties.spawnDummies ?? false;
        if (spawnDummies)
        {
            List<Transform> spawnLocations = new(shipStatus.DummyLocations);
            for (var i = 0; i < DUMMY_SPAWN_COUNT; i++)
            {
                var vector = Vector2.up;
                vector = vector.Rotate(i * (360f / DUMMY_SPAWN_COUNT));
                vector *= shipStatus.SpawnRadius;

                GameObject dummy = new($"Spawn Dummy {i + 1}");
                dummy.transform.position = gameObject.transform.position + (Vector3)vector;
                spawnLocations.Add(dummy.transform);
            }

            shipStatus.DummyLocations = spawnLocations.ToArray();
        }
    }

    [MapBuilder(
        Target = MapTarget.Game,
        Priority = Priority.LAST
    )]
    public static void AddFallbackSpawn(ShipStatus shipStatus)
    {
        if (!_hasMeetingSpawn)
        {
            shipStatus.MeetingSpawnCenter = _fallbackSpawn;
            shipStatus.MeetingSpawnCenter2 = _fallbackSpawn;
        }

        if (!_hasInitialSpawn) shipStatus.InitialSpawnCenter = _fallbackSpawn;
    }
}