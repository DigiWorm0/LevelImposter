using HarmonyLib;
using LevelImposter.DB;
using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace LevelImposter.Builders
{
    class ShipRoomBuilder : Builder
    {
        private PolusHandler polus;
        private int roomId;
        public static Dictionary<long, SystemTypes> db;

        public ShipRoomBuilder(PolusHandler polus)
        {
            this.polus = polus;
            roomId = 1;
            db = new Dictionary<long, SystemTypes>();

            // Make Default Room
            GameObject defaultObj = new GameObject("Default Room");
            BoxCollider2D defaultCollider = defaultObj.AddComponent<BoxCollider2D>();
            PlainShipRoom defaultRoom = defaultObj.AddComponent<PlainShipRoom>();
            defaultRoom.RoomId = 0;
            defaultRoom.roomArea = defaultCollider;
            defaultObj.transform.SetParent(polus.gameObject.transform);
        }

        public bool Build(MapAsset asset)
        {
            // Check Collider Count
            if (asset.colliders.Length <= 0)
            {
                LILogger.LogWarn(asset.name + " does not have any colliders!");
                return false;
            }

            // Object
            GameObject obj = new GameObject(asset.name);

            // Collider
            EdgeCollider2D collider = obj.AddComponent<EdgeCollider2D>();
            collider.SetPoints(asset.colliders[0].GetPoints());
            collider.isTrigger = true;

            // Room
            PlainShipRoom room = obj.AddComponent<PlainShipRoom>();
            room.RoomId = (SystemTypes)roomId;
            room.roomArea = collider;

            // Room DB
            db.Add(asset.id, (SystemTypes)roomId);

            // Polus
            polus.shipStatus.AllRooms = AssetBuilder.AddToArr(polus.shipStatus.AllRooms, room);
            polus.shipStatus.FastRooms.Add((SystemTypes)roomId, room);
            MapGenerator.AddRoom(asset);
            polus.Add(obj, asset);

            roomId++;
            return true;
        }
    }
}
