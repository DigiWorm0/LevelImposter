using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using LevelImposter.Builders;
using LevelImposter.DB;
using LevelImposter.Shop;
using LevelImposter.Trigger;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LevelImposter.Core;

/// <summary>
///     Component adds additional functionality added to AU's built-in ShipStatus.
///     Always added to ShipStatus on Awake, but dormant until LoadMap is fired.
/// </summary>
public class LIShipStatus(IntPtr intPtr) : MonoBehaviour(intPtr)
{
    public static readonly Dictionary<string, string> EXILE_IDS = new()
    {
        { "Skeld", "ss-skeld" },
        { "MiraHQ", "ss-mira" },
        { "Polus", "ss-polus" },
        { "Airship", "ss-airship" },
        { "Fungle", "ss-fungle" }
    };

    public static readonly KeyCode[] RESPAWN_SEQ =
    {
        KeyCode.I,
        KeyCode.M,
        KeyCode.S,
        KeyCode.T,
        KeyCode.U,
        KeyCode.C,
        KeyCode.K
    };

    public static readonly KeyCode[] CPU_SEQ =
    {
        KeyCode.C,
        KeyCode.P,
        KeyCode.U,
        KeyCode.C,
        KeyCode.P,
        KeyCode.U
    };

    public static readonly KeyCode[] DEBUG_SEQ =
    {
        KeyCode.D,
        KeyCode.E,
        KeyCode.B,
        KeyCode.U,
        KeyCode.G
    };

    private static LIShipStatus? _instance;

    [Obsolete("Use LIShipStatus.IsInstance() or LIShipStatus.GetInstance() instead")]
    public static LIShipStatus? Instance => _instance;

    [HideFromIl2Cpp] public MapObjectDB MapObjectDB { get; } = new();
    [HideFromIl2Cpp] public TriggerSystem TriggerSystem { get; } = new();
    [HideFromIl2Cpp] public RenameHandler Renames { get; } = new();
    [HideFromIl2Cpp] public LIMap? CurrentMap { get; private set; }

    public bool IsBuilding { get; private set; } = true;
    public bool IsReady => !IsBuilding && !LoadingBar.IsVisible;

    public ShipStatus? ShipStatus { get; private set; }

    public void Awake()
    {
        ShipStatus = GetComponent<ShipStatus>();
        _instance = this;

        if (MapLoader.CurrentMap != null)
            LoadMap(MapLoader.CurrentMap);
        else
            LILogger.Warn("No map content, no LI map will load");
    }

    public void Start()
    {
        if (CurrentMap != null)
        {
            DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt("_Mask", 7);

            // Respawn the player on key combo
            StartCoroutine(CoHandleKeyCombo(RESPAWN_SEQ, () => { RespawnPlayer(PlayerControl.LocalPlayer); })
                .WrapToIl2Cpp());
        }
    }

    public void OnDestroy()
    {
        _instance = null;

        // Wipe Cache (Freeplay Only)
        if (GameState.IsInFreeplay && LIConstants.FREEPLAY_FLUSH_CACHE)
            GCHandler.Clean();
    }

    /// <summary>
    ///     Gets the current LevelImposter instance or throws MissingShip if not found
    /// </summary>
    /// <returns>The current LevelImposter instance</returns>
    /// <exception cref="MissingShipException">If the instance is null or missing</exception>
    public static LIShipStatus GetInstance()
    {
        if (_instance == null)
            throw new MissingShipException();
        return _instance;
    }

    /// <summary>
    ///     Gets the current LevelImposter instance or null if not found
    /// </summary>
    /// <returns>The current LevelImposter instance or null</returns>
    public static LIShipStatus? GetInstanceOrNull()
    {
        return _instance;
    }

    /// <summary>
    ///     Gets ShipStatus or throws MissingShip if not found
    /// </summary>
    /// <returns>The local ShipStatus</returns>
    /// <exception cref="MissingShipException">ShipStatus or LIShipStatus is null</exception>
    public static ShipStatus GetShip()
    {
        var instance = GetInstance().ShipStatus;
        if (instance == null)
            throw new MissingShipException();
        return instance;
    }

    /// <summary>
    ///     Checks if the player is currently in a LevelImposter map.
    /// </summary>
    /// <returns>True if the player is in a LevelImposter map, false otherwise</returns>
    public static bool IsInstance()
    {
        return _instance != null;
    }

    /// <summary>
    ///     Resets the map to a blank slate. Ran before any map elements are applied.
    /// </summary>
    public void ResetMap()
    {
        if (ShipStatus == null)
            return;

        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
        Destroy(GetComponent<TagAmbientSoundPlayer>());

        if (Camera.main == null)
            throw new Exception("Main Camera is missing");
        var camera = Camera.main.GetComponent<FollowerCamera>();
        camera.shakeAmount = 0;
        camera.shakePeriod = 0;

        ShipStatus.AllDoors = new Il2CppReferenceArray<OpenableDoor>(0);
        ShipStatus.DummyLocations = new Il2CppReferenceArray<Transform>(0);
        ShipStatus.SpecialTasks = new Il2CppReferenceArray<PlayerTask>(0);
        ShipStatus.CommonTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        ShipStatus.LongTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        ShipStatus.ShortTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        ShipStatus.SystemNames = new Il2CppStructArray<StringNames>(0);
        ShipStatus.Systems = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>();
        ShipStatus.MedScanner = null;
        ShipStatus.Type = (ShipStatus.MapType)MapType.LevelImposter;
        ShipStatus.WeaponsImage = null;
        ShipStatus.EmergencyButton = null;

        ShipStatus.InitialSpawnCenter = new Vector2(0, 0);
        ShipStatus.MeetingSpawnCenter = new Vector2(0, 0);
        ShipStatus.MeetingSpawnCenter2 = new Vector2(0, 0);

        ShipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
        ShipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
        ShipStatus.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().Cast<ISystemType>()); // (Default)
        ShipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
        ShipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
        ShipStatus.Systems.Add(SystemTypes.Reactor,
            new ReactorSystemType(45f, SystemTypes.Reactor).Cast<ISystemType>()); // <- Seconds, SystemType
        ShipStatus.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(45f).Cast<ISystemType>()); // <- Seconds
        ShipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
        ShipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new[]
        {
            ShipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
            ShipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
            ShipStatus.Systems[SystemTypes.Reactor].Cast<IActivatable>(),
            ShipStatus.Systems[SystemTypes.LifeSupp].Cast<IActivatable>()
        }).Cast<ISystemType>());

        Renames.Clear();
        SystemDistributor.Reset();
    }

    /// <summary>
    ///     Replaces the active map with LevelImposter map data
    /// </summary>
    /// <param name="map">Deserialized map data from a <c>.LIM</c> file</param>
    [HideFromIl2Cpp]
    public void LoadMap(LIMap map)
    {
        LILogger.Msg($"Loading {map}");
        IsBuilding = true;
        LoadingBar.Run();
        CurrentMap = map;
        ResetMap();
        LoadMapProperties(map);
        BuildRouter buildRouter = new();

        // Asset DB
        if (!AssetDB.IsInit)
            LILogger.Warn("Asset DB is not initialized yet!");

        // Create GameObjects
        foreach (var elem in map.elements)
        {
            // Create GameObject
            var objName = elem.name.Replace("\\n", " ");
            var elemObject = new GameObject(objName);
            elemObject.transform.SetParent(transform);

            // Append MapObjectData component
            var mapObjectData = elemObject.AddComponent<MapObjectData>();
            mapObjectData.SetSourceElement(elem);

            // Add to DB
            MapObjectDB.AddObject(elem.id, elemObject);
        }

        // Set Parenting
        foreach (var elem in map.elements)
        {
            // Get Element Properties
            var elemObject = MapObjectDB.GetObject(elem.id);
            if (elemObject == null)
                continue;

            // Get Parent ID
            var parent = elem.parentID;
            if (parent == null)
                continue;

            // Find Parent Object
            var parentObject = MapObjectDB.GetObject((Guid)parent);
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

        IsBuilding = false;
        LILogger.Msg("Done");
    }

    /// <summary>
    ///     Loads LIMap.properties to ShipStatus
    /// </summary>
    /// <param name="map">Map to read properties from</param>
    [HideFromIl2Cpp]
    private void LoadMapProperties(LIMap map)
    {
        if (ShipStatus == null)
            return;

        ShipStatus.name = map.name;

        if (!string.IsNullOrEmpty(map.properties.bgColor))
            if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out var bgColor))
                Camera.main.backgroundColor = bgColor;

        if (!string.IsNullOrEmpty(map.properties.exileID))
        {
            if (!EXILE_IDS.ContainsKey(map.properties.exileID))
            {
                LILogger.Warn($"Unknown exile ID: {map.properties.exileID}");
                return;
            }

            var prefabShip = AssetDB.GetObject(EXILE_IDS[map.properties.exileID]);
            var prefabShipStatus = prefabShip?.GetComponent<ShipStatus>();
            if (prefabShipStatus == null)
                return;
            ShipStatus.ExileCutscenePrefab = prefabShipStatus.ExileCutscenePrefab;
        }
    }

    /// <summary>
    ///     Coroutine to respawn the player
    ///     with a specific key combo. (Shift + "RES" or Shift + "CPU")
    /// </summary>
    [HideFromIl2Cpp]
    private IEnumerator CoHandleKeyCombo(KeyCode[] sequence, Action onSequence)
    {
        var state = 0;

        while (true)
        {
            var shift = Input.GetKey(KeyCode.LeftShift)
                        || Input.GetKey(KeyCode.RightShift);
            var seqKey = Input.GetKeyDown(sequence[state]);
            var backKey = Input.GetKeyDown(KeyCode.Backspace);

            if (shift && seqKey)
                state++;
            else if (!shift || backKey) state = 0;

            if (state >= sequence.Length)
            {
                state = 0;
                onSequence.Invoke();
            }

            yield return null;
        }
    }

    /// <summary>
    ///     Resets the Local Player to the
    ///     ShipStatus's spawn location in the
    ///     event they are stuck.
    /// </summary>
    /// <param name="playerControl">Player to respawn</param>
    [MethodRpc((uint)LIRpc.ResetPlayer)]
    private static void RespawnPlayer(PlayerControl playerControl)
    {
        var shipStatus = _instance?.ShipStatus;
        if (playerControl == null || shipStatus == null)
            return;
        LILogger.Info($"Resetting {playerControl.name} to spawn");
        var playerPhysics = playerControl.GetComponent<PlayerPhysics>();
        playerPhysics.transform.position = shipStatus.InitialSpawnCenter;
        if (playerPhysics.AmOwner)
        {
            playerPhysics.ExitAllVents();
            LILogger.Notify("You've been reset to spawn", false);
        }
    }
}