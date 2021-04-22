using LevelImposter.Builders;
using LevelImposter.DB;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    class MapApplicator
    {
        public PolusHandler polus;
        public AssetBuilder builder;
        public MapData      map;

        public void PreBuild(PolusShipStatus shipStatus)
        {
            // Load Map and AssetDB
            AssetDB.FinishImport();
            if (!MapHandler.Load())
                return;

            // Vars
            map     = MapHandler.GetMap();
            polus   = new PolusHandler(shipStatus);
            builder = new AssetBuilder(polus);

            // Clear
            LILogger.LogInfo("...Clearing Polus");
            polus.ClearTasks();
            polus.MoveToTemp();
            polus.AddMissingProps(AssetDB.ss["ss-skeld"].ShipStatus);

            // Rooms
            LILogger.LogInfo("...Building Rooms");
            for (int i = 0; i < map.objs.Length; i++)
            {
                if (map.objs[i].type != "util-room")
                    continue;
                MapAsset asset = map.objs[i];
                bool success = builder.Build(asset);
                if (!success)
                    LILogger.LogError("Failed to build " + asset.name);
            }
            
            // Objects
            LILogger.LogInfo("...Building Objects");
            for (int i = 0; i < map.objs.Length; i++)
            {
                if (map.objs[i].type == "util-room")
                    continue;
                MapAsset asset = map.objs[i];
                bool success = builder.Build(asset);
                if (!success)
                    LILogger.LogError("Failed to build " + asset.name);
                else if (i % 100 == 0 && i != 0)
                    LILogger.LogInfo("..." + i + " Objects Built");
            }
        }

        public void PostBuild(PolusShipStatus shipStatus)
        {
            // Post Build
            LILogger.LogInfo("...Wrapping Up");
            VentBuilder.ConnectVents();
            builder.Finish();
            polus.DeleteTemp();
            polus.SetExile(MapHandler.mapData.exile);
            polus.minimap.Finish();
            LILogger.LogInfo("Finished!");
        }
    }
}
