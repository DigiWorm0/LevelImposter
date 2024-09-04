using System.Collections.Generic;
using LevelImposter.Core;
using UnityEngine;

namespace LevelImposter.Builders;

public class SpawnBuilder : IElemBuilder
{
    private const float DEFAULT_SPAWN_RADIUS = 1.55f;
    private const int DUMMY_SPAWN_COUNT = 15;
    private Vector2 _fallbackSpawn;

    private bool _hasInitialSpawn;
    private bool _hasMeetingSpawn;

    public void Build(LIElement elem, GameObject obj)
    {
        if (!elem.type.StartsWith("util-spawn"))
            return;

        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        // Set Spawn Radius
        shipStatus.SpawnRadius = elem.properties.range ?? DEFAULT_SPAWN_RADIUS;

        // Set Spawn Point
        Vector2 pos = obj.transform.position - new Vector3(0f, 0.3636f, 0f);
        _fallbackSpawn = pos;
        if (elem.type == "util-spawn1")
        {
            shipStatus.InitialSpawnCenter = pos;
            _hasInitialSpawn = true;
        }
        else if (elem.type == "util-spawn2")
        {
            shipStatus.MeetingSpawnCenter = pos;
            shipStatus.MeetingSpawnCenter2 = pos;
            _hasMeetingSpawn = true;
        }
        else
        {
            LILogger.Warn($"{elem.name} has an unknown spawn type");
        }

        // Add Dummy Locations
        var spawnDummies = elem.properties.spawnDummies ?? false;
        if (spawnDummies)
        {
            List<Transform> spawnLocations = new(shipStatus.DummyLocations);
            for (var i = 0; i < DUMMY_SPAWN_COUNT; i++)
            {
                var vector = Vector2.up;
                vector = vector.Rotate(i * (360f / DUMMY_SPAWN_COUNT));
                vector *= shipStatus.SpawnRadius;

                GameObject dummy = new($"Spawn Dummy {i + 1}");
                dummy.transform.position = obj.transform.position + (Vector3)vector;
                spawnLocations.Add(dummy.transform);
            }

            shipStatus.DummyLocations = spawnLocations.ToArray();
        }
    }

    public void PostBuild()
    {
        // ShipStatus
        var shipStatus = LIShipStatus.GetInstance().ShipStatus;

        if (!_hasMeetingSpawn)
        {
            shipStatus.MeetingSpawnCenter = _fallbackSpawn;
            shipStatus.MeetingSpawnCenter2 = _fallbackSpawn;
        }

        if (!_hasInitialSpawn) shipStatus.InitialSpawnCenter = _fallbackSpawn;
    }
}