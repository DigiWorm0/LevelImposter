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
        public MinimapGenerator minimap;

        public const int Y_OFFSET = 25; // Must be 0 < Y_OFFSET < 50

        public PolusHandler(PolusShipStatus shipStatus)
        {
            this.shipStatus = shipStatus;
            this.gameObject = shipStatus.gameObject;
            minimap = new MinimapGenerator(shipStatus.MapPrefab);
        }

        public void Add(GameObject obj, MapAsset asset, MapType map)
        {
            obj.transform.position = new Vector3(asset.x, -asset.y - Y_OFFSET, asset.z);
            obj.transform.localScale = new Vector3(
                asset.xScale * (asset.flipX ? -1 : 1) * (map == MapType.Airship ? 0.75f : 1),
                asset.yScale * (asset.flipY ? -1 : 1) * (map == MapType.Airship ? 0.75f : 1),
                1.0f
            );
            obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -asset.rotation));
            obj.transform.SetParent(gameObject.transform);
        }

        public void ClearTasks()
        {
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
            //shipStatus.Systems = new Il2CppSystem.Collections.Generic.Dictionary<SystemTypes, ISystemType>();
            shipStatus.SystemNames = new UnhollowerBaseLib.Il2CppStructArray<StringNames>(0);
            shipStatus.InitialSpawnCenter = new Vector2(0, -Y_OFFSET);
            shipStatus.MeetingSpawnCenter = new Vector2(0, -Y_OFFSET);
            shipStatus.MeetingSpawnCenter2 = new Vector2(0, -Y_OFFSET);
        }

        public void MoveToTemp()
        {
            Transform polusTransform = gameObject.transform;
            GameObject temp = new GameObject("temp");
            for (int i = polusTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = polusTransform.GetChild(i);
                child.SetParent(temp.transform);
            }
            temp.transform.SetParent(polusTransform);
        }

        public void DeleteTemp()
        {
            GameObject.Destroy(
                GameObject.Find("temp")
            );
        }

        public void AddMissingProps(ShipStatus shipStatus)
        {
            this.shipStatus.VentEnterSound = shipStatus.VentEnterSound;
            this.shipStatus.VentMoveSounds = shipStatus.VentMoveSounds;
        }
    }
}
