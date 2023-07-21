using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LevelImposter.Shop;
using LevelImposter.DB;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Reactor.Networking.Attributes;
using LevelImposter.Builders;

namespace LevelImposter.Core
{
    /// <summary>
    /// Component adds additional functionality added to AU's built-in ShipStatus.
    /// Always added to ShipStatus on Awake, but dormant until LoadMap is fired.
    /// </summary>
    public class LIShipStatus : MonoBehaviour
    {
        public LIShipStatus(IntPtr intPtr) : base(intPtr)
        {
        }

        public static LIShipStatus? Instance { get; private set; }

        public static readonly List<string> PRIORITY_TYPES = new()
        {
            "util-minimap",
            "util-room"
        };
        public static readonly Dictionary<string, string> EXILE_IDS = new()
        {
            { "Skeld", "ss-skeld" },
            { "MiraHQ", "ss-mira" },
            { "Polus", "ss-polus" },
            { "Airship", "ss-airship" }
        };
        public static readonly KeyCode[] RESPAWN_SEQ = new KeyCode[] {
            KeyCode.R,
            KeyCode.E,
            KeyCode.S
        };
        public static readonly KeyCode[] CPU_SEQ = new KeyCode[] {
            KeyCode.C,
            KeyCode.P,
            KeyCode.U,
            KeyCode.C,
            KeyCode.P,
            KeyCode.U
        };
        public static readonly KeyCode[] DEBUG_SEQ = new KeyCode[] {
            KeyCode.D,
            KeyCode.E,
            KeyCode.B,
            KeyCode.U,
            KeyCode.G
        };

        private RenameHandler _renames = new();
        private LIMap? _currentMap = null;
        private ShipStatus? _shipStatus = null;
        private bool _isReady = true;

        [HideFromIl2Cpp] public RenameHandler Renames => _renames;
        [HideFromIl2Cpp] public LIMap? CurrentMap => _currentMap;
        public ShipStatus? ShipStatus => _shipStatus;
        public bool IsReady
        {
            get {
                return SpriteLoader.Instance?.RenderCount <= 0 &&
                        !MapSync.IsDownloadingMap &&
                        _isReady;
            }
        }
        
        /// <summary>
        /// Resets the map to a blank slate. Ran before any map elements are applied.
        /// </summary>
        public void ResetMap()
        {
            if (ShipStatus == null)
                return;

            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
            Destroy(GetComponent<TagAmbientSoundPlayer>());

            FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
            camera.shakeAmount = 0;
            camera.shakePeriod = 0;

            ShipStatus.AllDoors = new Il2CppReferenceArray<PlainDoor>(0);
            ShipStatus.DummyLocations = new Il2CppReferenceArray<Transform>(0);
            ShipStatus.SpecialTasks = new Il2CppReferenceArray<PlayerTask>(0);
            ShipStatus.CommonTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
            ShipStatus.LongTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
            ShipStatus.NormalTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
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
            ShipStatus.Systems.Add(SystemTypes.Reactor, new ReactorSystemType(45f, SystemTypes.Reactor).Cast<ISystemType>()); // <- Seconds, SystemType
            ShipStatus.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(45f).Cast<ISystemType>()); // <- Seconds
            ShipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                ShipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.Reactor].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.LifeSupp].Cast<IActivatable>(),
            }).Cast<ISystemType>());

            _renames.Clear();
            SystemDistributor.Reset();
        }

        /// <summary>
        /// Replaces the active map with LevelImposter map data
        /// </summary>
        /// <param name="map">Deserialized map data from a <c>.LIM</c> file</param>
        [HideFromIl2Cpp]
        public void LoadMap(LIMap map)
        {
            LILogger.Info($"Loading {map}");
            _isReady = false;
            StartCoroutine(CoLoadingScreen().WrapToIl2Cpp());
            _currentMap = map;
            ResetMap();
            LoadMapProperties(map);
            BuildRouter buildRouter = new();

            // Asset DB
            if (!AssetDB.IsInit)
                LILogger.Warn("Asset DB is not initialized yet!");

            // Sprite Loader
            SpriteLoader.Instance?.SearchForDuplicateSprites(map);

            // Priority First
            foreach (string type in PRIORITY_TYPES)
                foreach (LIElement elem in map.elements)
                    if (elem.type == type)
                        AddElement(buildRouter, elem);
            // Everything Else
            foreach (LIElement elem in map.elements)
                if (!PRIORITY_TYPES.Contains(elem.type))
                    AddElement(buildRouter, elem);

            buildRouter.PostBuild();
            _isReady = true;
            LILogger.Info("Map load completed");
        }

        /// <summary>
        /// Loads LIMap.properties to ShipStatus
        /// </summary>
        /// <param name="map">Map to read properties from</param>
        [HideFromIl2Cpp]
        private void LoadMapProperties(LIMap map)
        {
            if (ShipStatus == null)
                return;

            ShipStatus.name = map.name;

            if (!string.IsNullOrEmpty(map.properties.bgColor))
            {
                if (ColorUtility.TryParseHtmlString(map.properties.bgColor, out Color bgColor))
                    Camera.main.backgroundColor = bgColor;
            }

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
        /// Adds a single <c>LIElement</c> object into the map.
        /// </summary>
        /// <param name="element"></param>
        [HideFromIl2Cpp]
        public void AddElement(BuildRouter buildRouter, LIElement element)
        {
            Stopwatch buildTimer = Stopwatch.StartNew();

            LILogger.Info($"Adding {element}");
            try
            {
                GameObject gameObject = buildRouter.Build(element);
                gameObject.transform.SetParent(transform);
                gameObject.transform.localPosition = MapUtils.ScaleZPositionByY(gameObject.transform.localPosition);
            }
            catch (Exception e)
            {
                LILogger.Error($"Error while building {element.name}:\n{e}");
            }

            // Build Timer
            buildTimer.Stop();
            if (buildTimer.ElapsedMilliseconds > LIConstants.ELEM_WARN_TIME)
                LILogger.Warn($"Took {buildTimer.ElapsedMilliseconds}ms to build {element.name}");
        }

        /// <summary>
        /// Coroutine that displayes the loading screen until map is built
        /// </summary>
        [HideFromIl2Cpp]
        private IEnumerator CoLoadingScreen()
        {
            yield return null;

            // Objects
            bool isFreeplay = GameState.IsInFreeplay;
            SpriteRenderer fullScreen = DestroyableSingleton<HudManager>.Instance.FullScreen;
            GameObject loadingBean = DestroyableSingleton<HudManager>.Instance.GameLoadAnimation;
            Color sabColor = fullScreen.color;

            // Loading
            LILogger.Info($"Showing loading screen (Freeplay={isFreeplay})");
            if (isFreeplay)
            {
                fullScreen.color = new Color(0, 0, 0, 0.9f);
                fullScreen.gameObject.SetActive(true);
            }
            while (!IsReady)
            {
                loadingBean.SetActive(true);
                yield return null;
            }

            // Sabotage
            LILogger.Info($"Hiding loading screen (Freeplay={isFreeplay})");
            if (isFreeplay)
            {
                fullScreen.color = sabColor;
                fullScreen.gameObject.SetActive(false);
            }
            loadingBean.SetActive(false);
        }

        /// <summary>
        /// Coroutine to respawn the player
        /// with a specific key combo. (Shift + "RES" or Shift + "CPU")
        /// </summary>
        [HideFromIl2Cpp]
        private IEnumerator CoHandleKeyCombo(KeyCode[] sequence, Action onSequence)
        {
            int state = 0;

            while (true)
            {
                bool shift = Input.GetKey(KeyCode.LeftShift)
                        || Input.GetKey(KeyCode.RightShift);
                bool seqKey = Input.GetKeyDown(sequence[state]);
                bool backKey = Input.GetKeyDown(KeyCode.Backspace);

                if (shift && seqKey)
                {
                    state++;
                }
                else if (!shift || backKey)
                {
                    state = 0;
                }

                if (state >= sequence.Length)
                {
                    state = 0;
                    onSequence.Invoke();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Resets the Local Player to the
        /// ShipStatus's spawn location in the
        /// event they are stuck.
        /// </summary>
        /// <param name="playerControl">Player to respawn</param>
        [MethodRpc((uint)LIRpc.ResetPlayer)]
        private static void RespawnPlayer(PlayerControl playerControl)
        {
            ShipStatus? shipStatus = Instance?.ShipStatus;
            if (playerControl == null || shipStatus == null)
                return;
            LILogger.Info($"Resetting {playerControl.name} to spawn");
            PlayerPhysics playerPhysics = playerControl.GetComponent<PlayerPhysics>();
            playerPhysics.transform.position = shipStatus.InitialSpawnCenter;
            if (playerPhysics.AmOwner)
            {
                playerPhysics.ExitAllVents();
                LILogger.Notify("<color=green>You've been reset to spawn</color>");
            }
        }

        /// <summary>
        /// Sets the CPU Affinity of Among Us to CPUs 1 and 2 as per 
        /// https://github.com/eDonnes124/Town-Of-Us-R/issues/81
        /// </summary>
        private static void SetCPUAffinity()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var process = Process.GetCurrentProcess();

            if (!isWindows && !isLinux)
            {
                LILogger.Notify("<color=red>System Incompatible</color>");
            }
            else if (process.ProcessorAffinity.ToInt32() == 6)
            {
                process.ProcessorAffinity = (IntPtr)(Math.Pow(2, Environment.ProcessorCount) - 1);
                LILogger.Notify("<color=red>Reset CPU affinity</color>");
            }
            else
            {
                process.ProcessorAffinity = (IntPtr)6;
                LILogger.Notify("<color=green>Set CPU affinity to 1 & 2</color>");
            }
        }

        private static void RunDebugTests()
        {
            // ============= Insert Debug Tests Here =============
        }

        public void Awake()
        {
            _shipStatus = GetComponent<ShipStatus>();
            Instance = this;

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
                StartCoroutine(CoHandleKeyCombo(RESPAWN_SEQ, () =>{
                    RespawnPlayer(PlayerControl.LocalPlayer);
                }).WrapToIl2Cpp());

                // Set CPU affinity on key combo
                StartCoroutine(CoHandleKeyCombo(CPU_SEQ, () => {
                    SetCPUAffinity();
                }).WrapToIl2Cpp());

                // Run Debug Tests
                StartCoroutine(CoHandleKeyCombo(DEBUG_SEQ, () => {
                    RunDebugTests();
                }).WrapToIl2Cpp());
            }
        }
        public void OnDestroy()
        {
            _renames = null;
            _currentMap = null;
            Instance = null;
            
            // Wipe Cache (Freeplay Only)
            if (GameState.IsInFreeplay)
                GCHandler.Clean();
        }
    }
}
