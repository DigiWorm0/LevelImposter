using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;
using LevelImposter.DB;
using System.Diagnostics;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

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

        public const int Y_OFFSET = 0; // 25
        public const float PLAYER_POS = -5.0f;

        public static LIShipStatus Instance { get; private set; }

        public ShipStatus ShipStatus { get; private set; }
        public LIMap CurrentMap { get; private set; }

        private BuildRouter _buildRouter = new BuildRouter();
        private Stopwatch _buildTimer = new Stopwatch();
        private Stack<Texture2D> _mapTextures = new Stack<Texture2D>();
        private Stack<AudioClip> _mapClips = new Stack<AudioClip>();

        private readonly List<string> _priorityTypes = new()
        {
            "util-minimap",
            "util-room"
        };
        private readonly Dictionary<string, string> _exileIDs = new()
        {
            { "Skeld", "ss-skeld" },
            { "MiraHQ", "ss-mira" },
            { "Polus", "ss-polus" },
            { "Airship", "ss-airship" }
        };

        public void Awake()
        {
            Destroy(GetComponent<TagAmbientSoundPlayer>());

            ShipStatus = GetComponent<ShipStatus>();
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
            LILogger.Info("Destroying " + _mapTextures.Count + " map textures");
            while (_mapTextures.Count > 0)
                Destroy(_mapTextures.Pop());

            LILogger.Info("Destroying " + _mapClips.Count + " map sounds");
            while (_mapClips.Count > 0)
                Destroy(_mapClips.Pop());
        }
        
        /// <summary>
        /// Resets the map to a blank slate. Ran before any map elements are applied.
        /// </summary>
        public void ResetMap()
        {
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

            ShipStatus.InitialSpawnCenter = new Vector2(0, -Y_OFFSET);
            ShipStatus.MeetingSpawnCenter = new Vector2(0, -Y_OFFSET);
            ShipStatus.MeetingSpawnCenter2 = new Vector2(0, -Y_OFFSET);

            ShipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Doors, new DoorsSystemType().Cast<ISystemType>()); // <-- Doors w/ Task
            //shipStatus.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().Cast<ISystemType>()); // <-- Doors w/o Task
            ShipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory).Cast<ISystemType>()); // <- Seconds, SystemType
            ShipStatus.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(60f).Cast<ISystemType>()); // <- Seconds
            ShipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
            ShipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                ShipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                ShipStatus.Systems[SystemTypes.Laboratory].Cast<IActivatable>()
            }).Cast<ISystemType>());

            MapUtils.SystemRenames.Clear();
            MapUtils.TaskRenames.Clear();
        }

        /// <summary>
        /// Replaces the active map with LevelImposter map data
        /// </summary>
        /// <param name="map">Deserialized map data from a <c>.LIM</c> file</param>
        public void LoadMap(LIMap map)
        {
            LILogger.Info("Loading " + map.name + " [" + map.id + "]");
            CurrentMap = map;
            ResetMap();
            _LoadMapProperties(map);
            _buildRouter.ResetStack();

            // Asset DB
            if (!AssetDB.IsReady)
                LILogger.Warn("Asset DB is not ready yet!");

            // Priority First
            foreach (string type in _priorityTypes)
                foreach (LIElement elem in map.elements)
                    if (elem.type == type)
                        AddElement(elem);
            // Everything Else
            foreach (LIElement elem in map.elements)
                if (!_priorityTypes.Contains(elem.type))
                    AddElement(elem);

            _buildRouter.PostBuild();
            LILogger.Info("Map load completed");
        }

        private void _LoadMapProperties(LIMap map)
        {
            ShipStatus.name = map.name;

            if (!string.IsNullOrEmpty(map.properties.bgColor))
            {
                Color bgColor;
                ColorUtility.TryParseHtmlString(map.properties.bgColor, out bgColor);
                Camera.main.backgroundColor = bgColor;
            }

            if (!string.IsNullOrEmpty(map.properties.exileID))
            {
                if (_exileIDs.ContainsKey(map.properties.exileID))
                {
                    ShipStatus ship = AssetDB.Ships[_exileIDs[map.properties.exileID]].ShipStatus;
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
        public void AddElement(LIElement element)
        {
            _buildTimer.Restart();

            LILogger.Info("Adding " + element.ToString());
            try
            {
                GameObject gameObject = _buildRouter.Build(element);
                gameObject.transform.SetParent(transform);
                gameObject.transform.localPosition -= new Vector3(0, Y_OFFSET, -((element.y - Y_OFFSET) / 1000.0f) + PLAYER_POS);
            }
            catch (Exception e)
            {
                LILogger.Error("Error while building " + element.name + ":\n" + e);
            }

            _buildTimer.Stop();
            if (_buildTimer.ElapsedMilliseconds > 1000)
            {
                float seconds = Mathf.Round(_buildTimer.ElapsedMilliseconds / 100f) / 10f;
                LILogger.Warn("Took " + seconds + "s to build " + element.name);
            }
        }

        /// <summary>
        /// Adds a map texture to be garbage collected on unload
        /// </summary>
        /// <param name="tex">Texture to mark for garbage collection on unload</param>
        public void AddMapTexture(Texture2D tex)
        {
            _mapTextures.Push(tex);
        }

        /// <summary>
        /// Adds a map audio clip to be garbage collected on unload
        /// </summary>
        /// <param name="clip">Audio clip to mark for garbage collection on unload</param>
        public void AddMapSound(AudioClip clip)
        {
            _mapClips.Push(clip);
        }
    }
}
