using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;
using LevelImposter.DB;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine.SceneManagement;
using BepInEx.Unity.IL2CPP.Utils.Collections;

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

        public const float PLAYER_POS = -5.0f;
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

        private LIMap? _currentMap = null;
        private ShipStatus? _shipStatus = null;
        private bool _isReady = true;

        [HideFromIl2Cpp]
        public LIMap? CurrentMap => _currentMap;
        public ShipStatus? ShipStatus => _shipStatus;
        public bool IsReady
        {
            get { return SpriteLoader.Instance?.RenderCount <= 0 && WAVLoader.Instance?.LoadCount <= 0 && _isReady; }
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
            ShipStatus.Type = ShipStatus.MapType.Ship;
            ShipStatus.WeaponsImage = null;

            ShipStatus.InitialSpawnCenter = new Vector2(0, 0);
            ShipStatus.MeetingSpawnCenter = new Vector2(0, 0);
            ShipStatus.MeetingSpawnCenter2 = new Vector2(0, 0);

            ShipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().Cast<ISystemType>()); // (Default)
            ShipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(45f, SystemTypes.Laboratory).Cast<ISystemType>()); // <- Seconds, SystemType
            ShipStatus.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(45f).Cast<ISystemType>()); // <- Seconds
            ShipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                ShipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.Laboratory].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.LifeSupp].Cast<IActivatable>(),
            }).Cast<ISystemType>());

            MapUtils.SystemRenames.Clear();
            MapUtils.TaskRenames.Clear();
        }

        /// <summary>
        /// Replaces the active map with LevelImposter map data
        /// </summary>
        /// <param name="map">Deserialized map data from a <c>.LIM</c> file</param>
        [HideFromIl2Cpp]
        public void LoadMap(LIMap map)
        {
            LILogger.Info("Loading " + map.name + " [" + map.id + "]");
            _isReady = false;
            StartCoroutine(CoLoadingScreen().WrapToIl2Cpp());
            _currentMap = map;
            ResetMap();
            _LoadMapProperties(map);
            BuildRouter buildRouter = new();

            // Asset DB
            if (!AssetDB.IsReady)
                LILogger.Warn("Asset DB is not ready yet!");

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
        private void _LoadMapProperties(LIMap map)
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
                if (EXILE_IDS.ContainsKey(map.properties.exileID))
                {
                    ShipStatus ship = AssetDB.Ships[EXILE_IDS[map.properties.exileID]].ShipStatus;
                    ShipStatus.ExileCutscenePrefab = ship.ExileCutscenePrefab;
                }
                else
                {
                    LILogger.Warn("Unknown exile ID: " + map.properties.exileID);
                }
            }
        }

        /// <summary>
        /// Adds a single <c>LIElement</c> object into the map.
        /// </summary>
        /// <param name="element"></param>
        [HideFromIl2Cpp]
        public void AddElement(BuildRouter buildRouter, LIElement element)
        {
            Stopwatch buildTimer = new();
            buildTimer.Restart();

            LILogger.Info("Adding " + element.ToString());
            try
            {
                GameObject gameObject = buildRouter.Build(element);
                gameObject.transform.SetParent(transform);
                gameObject.transform.localPosition -= new Vector3(0, 0, -(element.y / 1000.0f) + PLAYER_POS);
            }
            catch (Exception e)
            {
                LILogger.Error("Error while building " + element.name + ":\n" + e);
            }

            // Build Timer
            buildTimer.Stop();
            if (buildTimer.ElapsedMilliseconds > 100)
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
            bool isFreeplay = AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
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
        /// with a specific key combo. (Ctrl + Shift + "RESPAWN")
        /// </summary>
        [HideFromIl2Cpp]
        private IEnumerator CoHandleRespawn()
        {
            int state = 0;
            KeyCode[] sequence = new KeyCode[]
            {
                KeyCode.R,
                KeyCode.E,
                KeyCode.S,
                KeyCode.P,
                KeyCode.A,
                KeyCode.W,
                KeyCode.N
            };

            while (true)
            {
                bool ctrl = Input.GetKey(KeyCode.LeftControl)
                         || Input.GetKey(KeyCode.RightControl);
                bool shift = Input.GetKey(KeyCode.LeftShift)
                        || Input.GetKey(KeyCode.RightShift);
                bool seqKey = Input.GetKeyDown(sequence[state]);

                if (ctrl && shift && seqKey)
                {
                    state++;
                }
                else if (!(ctrl || shift))
                {
                    state = 0;
                }

                if (state >= sequence.Length)
                {
                    state = 0;
                    RespawnPlayer();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Resets the Local Player to the
        /// ShipStatus's spawn location in the
        /// event they are stuck.
        /// </summary>
        private void RespawnPlayer()
        {
            GameObject? playerObj = PlayerControl.LocalPlayer?.gameObject;
            if (playerObj == null)
                return;
            LILogger.Info("Resetting player to spawn");
            playerObj.transform.position = ShipStatus?.InitialSpawnCenter ?? transform.position;
            LILogger.Notify("<color=green>You've been reset to spawn</color>");
        }

        public void Awake()
        {
            _shipStatus = GetComponent<ShipStatus>();
            Instance = this;

            if (MapLoader.CurrentMap != null)
                LoadMap(MapLoader.CurrentMap);
            else
                LILogger.Info("No map content, no LI data will load");
        }
        public void Start()
        {
            if (CurrentMap != null)
            {
                HudManager.Instance.ShadowQuad.material.SetInt("_Mask", 7);
                StartCoroutine(CoHandleRespawn().WrapToIl2Cpp());
            }
        }
        public void OnDestroy()
        {
            _currentMap = null;
            Instance = null;
            SpriteLoader.Instance?.ClearAll();
            WAVLoader.Instance?.ClearAll();
        }
    }
}
