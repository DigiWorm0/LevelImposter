using LevelImposter.Models;
using LevelImposter.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelImposter.Map
{
    static class ItemDB
    {
        private static Dictionary<string, AssetInfo> db;

        public static void Init()
        {
            db = Resources.ItemDB
                .Split("\n")
                .Select(line => line.Split(','))
                .ToDictionary(
                    line => line[1],
                    line => new AssetInfo(line[0], line[2], int.Parse(line[3]), line[4])
                );
        }

        public static bool Contains(string id)
        {
            return db.ContainsKey(id);
        }

        public static AssetInfo Get(string id)
        {
            return db.GetValueOrDefault(id);
        }
    }
}
