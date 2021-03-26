using LevelImposter.Builders;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    class MapApplicator
    {
        public void Apply(PolusShipStatus shipStatus)
        {
            if (!MapHandler.Load())
                return;

            MapData map = MapHandler.GetMap();
            Polus polus = new Polus(shipStatus);

            // Move Map to Temp Storage
            TempHandler.Create(polus);

            // Add Asset
            foreach (MapAsset asset in map.objs)
            {
                // Generate Object
                GameObject obj;
                if (asset.type.Equals("existing"))
                    obj = TaskBuilder.Build(asset.data);
                else
                    obj = CustomBuilder.Build(asset.data);

                // Add To Polus
                obj.transform.SetParent(polus.gameObject.transform);
                obj.transform.position = new Vector3(asset.x,asset.y,asset.z);
            }
            
            // Clear Temp Storage
            TempHandler.Clear();
        }
    }
}
