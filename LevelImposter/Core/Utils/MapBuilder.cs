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

public static class MapBuilder
{
    public static bool IsBuilding { get; private set; }
    
    public static readonly BuildRouter MapBuildRouter = new([
        new MapPropertiesBuilder(),

        new TransformBuilder(),
        new SpriteBuilder(),
        new ColliderBuilder(),
        new MinigameSpriteBuilder(),
        new LayerBuilder(),

        new RoomBuilder(),
        new AdminMapBuilder(),
        new RoomNameBuilder(),

        new MinimapBuilder(),
        new DummyBuilder(),
        new UtilBuilder(),
        new SpawnBuilder(),
        new VentBuilder(),
        new CamBuilder(),
        new DisplayBuilder(),
        new TaskBuilder(),
        new DecBuilder(),
        new PhysicsObjectBuilder(),
        new MeetingOptionsBuilder(),
        new SabotageOptionsBuilder(),
        new OneWayColliderBuilder(),
        new DecontaminationBuilder(),
        new SporeBuilder(),
        new BinocularsBuilder(),
        new FilterBuilder(),
        new EjectBuilder(),
        new EjectDummyBuilder(),
        new EjectHandBuilder(),
        new ValueBuilder(),
        new PlayerMoverBuilder(),

        new SabBuilder(),
        new SabMixupBuilder(),
        new SabConsoleBuilder(),
        new SabMapBuilder(),
        new SabDoorBuilder(),

        new MinimapSpriteBuilder(),
        new LadderBuilder(),
        new PlatformBuilder(),
        new StarfieldBuilder(),
        new FloatBuilder(),
        new ScrollBuilder(),
        new AmbientSoundBuilder(),
        new StepSoundBuilder(),
        new TeleBuilder(),
        new TeleLinkBuilder(),
        new TriggerAreaBuilder(),
        new TriggerConsoleBuilder(),
        new TriggerStartBuilder(),
        new TriggerDeathBuilder(),
        new TriggerShakeBuilder(),
        new TriggerAnimBuilder(),

        new CustomTextBuilder(),
        new ColorBuilder()
    ]);
    

    /// <summary>
    ///     Resets the map to a blank slate. Ran before any map elements are applied.
    /// </summary>
    private static void ResetMap()
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
    /// Resets and rebuilds the active map based on
    /// <see cref="GameConfiguration.CurrentMap"/>.
    /// </summary>
    /// <exception cref="Exception">If GameConfiguration.CurrentMap is null</exception>
    public static void RebuildMap()
    {
        if (GameConfiguration.CurrentMap == null)
            throw new Exception("CurrentMap is null");
        
        ResetMap();
        BuildMap(GameConfiguration.CurrentMap);
    }

    /// <summary>
    ///     Replaces the active map with LevelImposter map data
    /// </summary>
    /// <param name="map">Deserialized map data from a <c>.LIM</c> file</param>
    private static void BuildMap(LIMap map)
    {
        // Check Asset DB
        if (!AssetDB.IsInit)
            throw new Exception("AssetDB is not initialized");
        
        // START
        IsBuilding = true;
        LILogger.Info($"Building map from {map}...");
        
        // Set GC Behavior
        GCHandler.SetDefaultBehavior(GCBehavior.DisposeOnMapUnload);

        // Show Loading Bar (Freeplay Only)
        if (GameState.IsInFreeplay)
            LoadingBar.Run();
        
        // Rebuild the map
        MapBuildRouter.BuildMap(map.elements, LIShipStatus.GetInstance().transform);
        
        // FINISH
        LILogger.Info($"Built map from {map}");
        IsBuilding = false;
    }
}