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

        private const float PLAYER_POS = -5.0f;
        private static readonly List<string> PRIORITY_TYPES = new()
        {
            "util-minimap",
            "util-room"
        };
        private static readonly Dictionary<string, string> EXILE_IDS = new()
        {
            { "Skeld", "ss-skeld" },
            { "MiraHQ", "ss-mira" },
            { "Polus", "ss-polus" },
            { "Airship", "ss-airship" }
        };

        private LIMap? _currentMap = null;
        private ShipStatus? _shipStatus = null;

        [HideFromIl2Cpp]
        public LIMap? CurrentMap => _currentMap;
        public ShipStatus? ShipStatus => _shipStatus;
        
        /// <summary>
        /// Resets the map to a blank slate. Ran before any map elements are applied.
        /// </summary>
        public void ResetMap()
        {
            if (ShipStatus == null)
                return;

            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

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
            ShipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory).Cast<ISystemType>()); // <- Seconds, SystemType
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
            StartCoroutine(CoLoadingScreen().WrapToIl2Cpp());
            _currentMap = map;
            ResetMap();
            _LoadMapProperties(map);
            BuildRouter buildRouter = new();
            buildRouter.ResetStack();

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
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
                yield break;

            yield return null;
            SpriteRenderer fullScreen = DestroyableSingleton<HudManager>.Instance.FullScreen;
            GameObject loadingBean = DestroyableSingleton<HudManager>.Instance.GameLoadAnimation;
            Color sabColor = fullScreen.color;
            
            // Loading
            fullScreen.color = new Color(0, 0, 0, 0.8f);
            loadingBean.SetActive(true);
            fullScreen.gameObject.SetActive(true);

            while (SpriteLoader.Instance.RenderCount > 0)
                yield return null;

            // Sabotage
            fullScreen.color = sabColor;
            loadingBean.SetActive(false);
            fullScreen.gameObject.SetActive(false);
        }

        public void Awake()
        {
            Destroy(GetComponent<TagAmbientSoundPlayer>());
            gameObject.AddComponent<SpriteLoader>();
            gameObject.AddComponent<WAVLoader>();
            _shipStatus = GetComponent<ShipStatus>();
            Instance = this;

            if (MapLoader.CurrentMap != null)
                LoadMap(MapLoader.CurrentMap);
            else
                LILogger.Info("No map content, no LI data will load");
        }
        public void Start()
        {
            if (MapLoader.CurrentMap != null)
                HudManager.Instance.ShadowQuad.material.SetInt("_Mask", 7);
        }
        public void OnDestroy()
        {
            _currentMap = null;
            Instance = null;
            GC.Collect();
        }
    }
}
