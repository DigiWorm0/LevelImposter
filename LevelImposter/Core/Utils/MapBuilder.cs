using System;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;
using LevelImposter.Builders;
using LevelImposter.DB;
using LevelImposter.Shop;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LevelImposter.Core;

public class MapBuilder
{
    public bool IsBuilding { get; private set; }

    /// <summary>
    ///     Resets the map to a blank slate. Ran before any map elements are applied.
    /// </summary>
    public void ResetMap()
    {
        // Get Ship Status
        var liShipStatus = LIShipStatus.GetInstance();
        var shipStatus = LIShipStatus.GetShip();
        if (shipStatus == null)
            return;

        // Remove All Children
        while (shipStatus.transform.childCount > 0)
            Object.DestroyImmediate(shipStatus.transform.GetChild(0).gameObject);

        // Remove Tag Ambient Sound Player
        Object.Destroy(shipStatus.GetComponent<TagAmbientSoundPlayer>());

        if (Camera.main == null)
            throw new Exception("Main Camera is missing");
        var camera = Camera.main.GetComponent<FollowerCamera>();
        camera.shakeAmount = 0;
        camera.shakePeriod = 0;

        shipStatus.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
        shipStatus.DummyLocations = new Il2CppReferenceArray<Transform>(0);
        shipStatus.SpecialTasks = new Il2CppReferenceArray<PlayerTask>(0);
        shipStatus.CommonTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        shipStatus.LongTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        shipStatus.ShortTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        shipStatus.SystemNames = new Il2CppStructArray<StringNames>(0);
        shipStatus.Systems = new Dictionary<SystemTypes, ISystemType>();
        shipStatus.MedScanner = null;
        shipStatus.Type = (ShipStatus.MapType)MapType.LevelImposter;
        shipStatus.WeaponsImage = null;
        shipStatus.EmergencyButton = null;

        shipStatus.InitialSpawnCenter = new Vector2(0, 0);
        shipStatus.MeetingSpawnCenter = new Vector2(0, 0);
        shipStatus.MeetingSpawnCenter2 = new Vector2(0, 0);

        shipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
        shipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
        shipStatus.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().Cast<ISystemType>()); // (Default)
        shipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
        shipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
        shipStatus.Systems.Add(SystemTypes.Reactor,
            new ReactorSystemType(45f, SystemTypes.Reactor).Cast<ISystemType>()); // <- Seconds, SystemType
        shipStatus.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(45f).Cast<ISystemType>()); // <- Seconds
        shipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
        shipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new[]
        {
            shipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
            shipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
            shipStatus.Systems[SystemTypes.Reactor].Cast<IActivatable>(),
            shipStatus.Systems[SystemTypes.LifeSupp].Cast<IActivatable>()
        }).Cast<ISystemType>());

        liShipStatus.Renames.Clear();
        SystemDistributor.Reset();
    }

    /// <summary>
    ///     Replaces the active map with LevelImposter map data
    /// </summary>
    /// <param name="map">Deserialized map data from a <c>.LIM</c> file</param>
    [HideFromIl2Cpp]
    public void BuildMap(LIMap map)
    {
        LILogger.Msg($"Loading {map}");
        IsBuilding = true;

        // Show Loading Bar (Freeplay Only)
        if (GameState.IsInFreeplay)
            LoadingBar.Run();

        // Get Ship Status
        var liShipStatus = LIShipStatus.GetInstance();

        ResetMap();
        BuildRouter buildRouter = new();

        // Asset DB
        if (!AssetDB.IsInit)
            LILogger.Warn("Asset DB is not initialized yet!");

        // Create GameObjects
        CreateGameObjects(map);

        // Prebuild
        LILogger.Msg("Running Pre-Build");
        foreach (var elem in map.elements)
            buildRouter.RunBuildStep(BuildRouter.BuildStep.PreBuild, elem);

        // Build
        LILogger.Msg("Running Build");
        foreach (var elem in map.elements)
            buildRouter.RunBuildStep(BuildRouter.BuildStep.Build, elem);

        // Postbuild
        LILogger.Msg("Running Post-Build");
        foreach (var elem in map.elements)
            buildRouter.RunBuildStep(BuildRouter.BuildStep.PostBuild, elem);

        // Cleanup
        LILogger.Msg("Running Cleanup");
        buildRouter.Cleanup();

        // Finish
        IsBuilding = false;
        LILogger.Msg("Done");
    }

    private void CreateGameObjects(LIMap map)
    {
        // Create each element's GameObject and MapObjectData
        var liShipStatus = LIShipStatus.GetInstance();
        foreach (var elem in map.elements)
        {
            // Create GameObject
            var objName = elem.name.Replace("\\n", " ");
            var elemObject = new GameObject(objName);
            elemObject.transform.SetParent(liShipStatus.transform);

            // Append MapObjectData component
            var mapObjectData = elemObject.AddComponent<MapObjectData>();
            mapObjectData.SetSourceElement(elem);

            // Add to DB
            liShipStatus.MapObjectDB.AddObject(elem.id, elemObject);
        }

        ApplyGameObjectHierarchy(map);
    }

    private void ApplyGameObjectHierarchy(LIMap map)
    {
        // Set Parent-Child Relationships
        var liShipStatus = LIShipStatus.GetInstance();
        foreach (var elem in map.elements)
        {
            // Get Element Properties
            var elemObject = liShipStatus.MapObjectDB.GetObject(elem.id);
            if (elemObject == null)
                continue;

            // Get Parent ID
            var parent = elem.parentID;
            if (parent == null)
                continue;

            // Find Parent Object
            var parentObject = liShipStatus.MapObjectDB.GetObject((Guid)parent);
            if (parentObject == null)
                continue;

            // Get Parent Element Properties
            var parentElement = parentObject.GetComponent<MapObjectData>();
            if (parentElement == null)
                continue;

            // Check if parent is a util-layer
            if (parentElement.Element.type != "util-layer")
                continue;

            // Set Parent
            elemObject.transform.SetParent(parentObject.transform);
        }
    }
}