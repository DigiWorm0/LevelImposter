using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LevelImposter.Shop;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class LIShipStatus : MonoBehaviour
    {
        public const int Y_OFFSET = 25;

        public static LIShipStatus Instance { get; private set; }
        public ShipStatus shipStatus { get; private set; }

        private LIMap currentMap;
        private BuildRouter buildRouter = new BuildRouter();

        public LIShipStatus(IntPtr intPtr) : base(intPtr)
        {
        }

        private void Awake()
        {
            shipStatus = GetComponent<ShipStatus>();
            Instance = this;
            SetMap(MapLoader.LoadMap("test1")); // <-- Temp
        }

        public void ResetMap()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            shipStatus.AllCameras = new UnhollowerBaseLib.Il2CppReferenceArray<SurvCamera>(0);
            shipStatus.AllDoors = new UnhollowerBaseLib.Il2CppReferenceArray<PlainDoor>(0);
            shipStatus.AllConsoles = new UnhollowerBaseLib.Il2CppReferenceArray<Console>(0);
            shipStatus.AllRooms = new UnhollowerBaseLib.Il2CppReferenceArray<PlainShipRoom>(0);
            shipStatus.AllStepWatchers = new UnhollowerBaseLib.Il2CppReferenceArray<IStepWatcher>(0);
            shipStatus.AllVents = new UnhollowerBaseLib.Il2CppReferenceArray<Vent>(0);
            shipStatus.DummyLocations = new UnhollowerBaseLib.Il2CppReferenceArray<Transform>(0);
            shipStatus.SpecialTasks = new UnhollowerBaseLib.Il2CppReferenceArray<PlayerTask>(0);
            shipStatus.CommonTasks = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            shipStatus.LongTasks = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            shipStatus.NormalTasks = new UnhollowerBaseLib.Il2CppReferenceArray<NormalPlayerTask>(0);
            shipStatus.FastRooms = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, PlainShipRoom>();
            shipStatus.SystemNames = new UnhollowerBaseLib.Il2CppStructArray<StringNames>(0);
            shipStatus.Systems = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>();

            shipStatus.InitialSpawnCenter = new Vector2(0, -Y_OFFSET);
            shipStatus.MeetingSpawnCenter = new Vector2(0, -Y_OFFSET);
            shipStatus.MeetingSpawnCenter2 = new Vector2(0, -Y_OFFSET);

            shipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
            //shipStatus.Systems.Add(SystemTypes.Doors, new DoorsSystemType().Cast<ISystemType>()); // <-- Doors w/ Task
            //shipStatus.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType().Cast<ISystemType>()); // <-- Doors w/o Task
            shipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory).Cast<ISystemType>()); // <- Seconds, SystemType
            shipStatus.Systems.Add(SystemTypes.Ventilation, new VentilationSystem().Cast<ISystemType>());
            shipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                shipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                shipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                shipStatus.Systems[SystemTypes.Laboratory].Cast<IActivatable>()
            }).Cast<ISystemType>());
        }

        public void SetMap(LIMap map)
        {
            AssetDB.Import();
            ResetMap();
            foreach (LIElement elem in map.elements)
                AddElement(elem);
            buildRouter.PostBuild();
            currentMap = map;
            LILogger.Msg("Added Map: " + map.name + " [" + map.authorId + "]");
        }

        public void AddElement(LIElement element)
        {
            GameObject gameObject = buildRouter.Build(element);
            gameObject.transform.SetParent(transform);
            gameObject.transform.localPosition -= new Vector3(0, Y_OFFSET, 0);
            LILogger.Msg("Added Element: " + element.name + " [" + element.type + "]");
        }

        public static UnhollowerBaseLib.Il2CppReferenceArray<T> AddToArr<T>(UnhollowerBaseLib.Il2CppReferenceArray<T> arr, T value) where T : UnhollowerBaseLib.Il2CppObjectBase
        {
            List<T> list = new List<T>(arr);
            list.Add(value);
            return list.ToArray();
        }
    }
}
