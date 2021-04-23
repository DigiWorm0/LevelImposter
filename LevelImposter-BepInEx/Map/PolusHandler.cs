using LevelImposter.DB;
using LevelImposter.MinimapGen;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    class PolusHandler
    {
        public PolusShipStatus shipStatus;
        public GameObject gameObject;
        public const int Y_OFFSET = 25; // Multiplayer Bug, Must be 0 < Y_OFFSET < 50

        public PolusHandler(PolusShipStatus shipStatus)
        {
            this.shipStatus = shipStatus;
            this.gameObject = shipStatus.gameObject;
            this.ClearShip();
            this.AddSystems();
        }

        // Add Objs
        public void Add(GameObject obj, MapAsset asset, float scale = 1.0f, float xOffset = 0, float yOffset = 0)
        {
            obj.transform.position = new Vector3(asset.x - xOffset, -asset.y - Y_OFFSET - yOffset, asset.z);
            obj.transform.localScale = new Vector3(
                asset.xScale * (asset.flipX ? -1 : 1) * scale,
                asset.yScale * (asset.flipY ? -1 : 1) * scale,
                1.0f
            );
            obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -asset.rotation));
            obj.transform.SetParent(gameObject.transform);
        }

        // Clear Ship
        private void ClearShip()
        {
            // Delete Children
            Transform polusTransform = gameObject.transform;
            for (int i = polusTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = polusTransform.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }

            // Ship Status
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

            // Spawn
            shipStatus.InitialSpawnCenter = new Vector2(0, -Y_OFFSET);
            shipStatus.MeetingSpawnCenter = new Vector2(0, -Y_OFFSET);
            shipStatus.MeetingSpawnCenter2 = new Vector2(0, -Y_OFFSET);
        }

        // Systems
        private void AddSystems()
        {
            // Sabotages
            this.shipStatus.Systems.Add(SystemTypes.Electrical, new SwitchSystem().Cast<ISystemType>());
            this.shipStatus.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType().Cast<ISystemType>());
            this.shipStatus.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory).Cast<ISystemType>());
            this.shipStatus.Systems.Add(SystemTypes.Doors, new DoorsSystemType().Cast<ISystemType>());
            this.shipStatus.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(new IActivatable[] {
                this.shipStatus.Systems[SystemTypes.Electrical].Cast<IActivatable>(),
                this.shipStatus.Systems[SystemTypes.Comms].Cast<IActivatable>(),
                this.shipStatus.Systems[SystemTypes.Laboratory].Cast<IActivatable>()
            }).Cast<ISystemType>());

            // Other
            this.shipStatus.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType().Cast<ISystemType>());
            this.shipStatus.Systems.Add(SystemTypes.MedBay, new MedScanSystem().Cast<ISystemType>());
        }

        public void SetExile(MapType type)
        {
            foreach (var ssKey in AssetDB.ss)
            {
                if (ssKey.Value.MapType == type)
                {
                    this.shipStatus.ExileCutscenePrefab = ssKey.Value.ShipStatus.ExileCutscenePrefab;
                }
            }
        }
    }
}
