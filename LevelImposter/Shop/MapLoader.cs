using System;
using System.Collections.Generic;
using System.Text;
using LevelImposter.Core;

namespace LevelImposter.Shop
{
    public class MapLoader
    {
        public static LIMap currentMap = null;

        public static void LoadMap(string mapID)
        {
            currentMap = MapFileAPI.Instance.Get(mapID);
        }

        public static void UnloadMap()
        {
            currentMap = null;
        }
    }
}
