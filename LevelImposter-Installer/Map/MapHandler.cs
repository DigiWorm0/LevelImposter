using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelImposter.Map
{
    static class MapHandler
    {
        public static event EventHandler onLoad;
        public static string mapDir;
        public static string name;

        public static void Load(string dir)
        {
            if (File.Exists(dir))
            {
                try
                {
                    mapDir = dir;
                    MapData mapData = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(File.ReadAllText(dir));
                    name = mapData.name;
                    onLoad(null, EventArgs.Empty);
                }
                catch
                {
                    Error("Invalid Map", "File is not a valid LevelImposter Map");
                }
            }
            else
            {
                Error("Invalid File", "Selected file does not exist");
            }
        }

        private static void Error(string title, string text)
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
