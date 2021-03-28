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

            MapData      map     = MapHandler.GetMap();
            PolusHandler polus   = new PolusHandler(shipStatus);
            AssetBuilder builder = new AssetBuilder(polus);
            
            polus.ClearTasks();
            polus.MoveToTemp();

            foreach (MapAsset asset in map.objs)
            {
                bool success = builder.Build(asset);
                if (!success)
                    LILogger.LogError("Failed to build " + asset.name);
            }

            polus.DeleteTemp();
            LILogger.LogInfo("Applied Map Data");
        }
    }
}
