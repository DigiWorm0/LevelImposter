using LevelImposter.DB;
using LevelImposter.MinimapGen;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Map
{
    static class TextHandler
    {
        public static Dictionary<SystemTypes, string> renames = new Dictionary<SystemTypes, string>();

        public static void Add(SystemTypes system, string name)
        {
            if (!renames.ContainsKey(system))
                renames.Add(system, name);
        }

        public static bool Contains(SystemTypes system)
        {
            return renames.ContainsKey(system);
        }

        public static string Get(SystemTypes system)
        {
            return renames[system];
        }
    }
}
