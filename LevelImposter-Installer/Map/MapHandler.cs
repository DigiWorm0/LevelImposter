using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Map
{
    static class MapHandler
    {
        public static event EventHandler onLoad;
        public static string mapDir;

        public static void Load(string dir)
        {
            mapDir = dir;
            onLoad(null, EventArgs.Empty);
        }
    }
}
