using LevelImposter.Builders;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.MinimapGen
{
    class AdminGenerator : Generator
    {
        private GameObject countObj;
        private MapCountOverlay count;

        public AdminGenerator(Minimap map)
        {
            countObj = map.prefab.transform.FindChild("CountOverlay").gameObject;
            count = countObj.GetComponent<MapCountOverlay>();
            count.CountAreas = new UnhollowerBaseLib.Il2CppReferenceArray<CounterArea>(0);
            MinimapGenerator.ClearChildren(countObj.transform);
        }

        public void Generate(MapAsset asset)
        {
            // Object
            GameObject counterObj = new GameObject(asset.name);
            counterObj.transform.position = new Vector3(
                asset.x * MinimapGenerator.MAP_SCALE,
                -asset.y * MinimapGenerator.MAP_SCALE - 0.25f,
                -25.0f
            );
            counterObj.transform.SetParent(countObj.transform);

            // Counter Area
            CounterArea counterArea = counterObj.AddComponent<CounterArea>();
            counterArea.RoomType = ShipRoomBuilder.db[asset.id];
            counterArea.MaxWidth = 5;
            counterArea.XOffset = 0.3f;
            counterArea.YOffset = 0.3f;
            counterArea.pool = countObj.GetComponent<ObjectPoolBehavior>();
            count.CountAreas = AssetBuilder.AddToArr(count.CountAreas, counterArea);
        }
    }
}
