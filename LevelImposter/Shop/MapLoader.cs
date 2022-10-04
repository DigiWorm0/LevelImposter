using System;
using System.Collections.Generic;
using System.Text;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class MapLoader
    {
        public static LIMap CurrentMap = null;

        public static void LoadMap(LIMap map)
        {
            CurrentMap = map;
        }

        public static void LoadMap(string mapID)
        {
            CurrentMap = MapFileAPI.Instance.Get(mapID);
        }

        public static void UnloadMap()
        {
            CurrentMap = null;
        }
    }
}
