using LevelImposter.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelImposter.Map
{
    class MapApplicator
    {
        const string DEFAULT_GAME_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Among Us\\Among Us.exe";
        public void Apply()
        {
            // Game Directory
            string gameExeDir = DEFAULT_GAME_DIR;
            if (!File.Exists(gameExeDir))
            {
                Error("Among Us Not Found", "Either Among Us is not installed or it is in a different location than Steam's default location.\nPlease find and select your Among Us.exe.");

                OpenFileDialog browseDialog = new OpenFileDialog();
                DialogResult result = browseDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    gameExeDir = browseDialog.FileName;
                    if (!File.Exists(browseDialog.FileName))
                    {
                        Error("Among Us Not Found", "Could not find Among Us installation.");
                        return;
                    }
                }
                else
                {
                    Error("Among Us Not Found", "Could not find Among Us installation.");
                    return;
                }
            }
            string installDir = Path.Combine(Path.GetDirectoryName(gameExeDir), "BepInEx\\plugins\\");
            string mapDir = Path.Combine(installDir, "map.json");
            string jsonDir = Path.Combine(installDir, "Newtonsoft.Json.dll");
            string liDir = Path.Combine(installDir, "LevelImposter.dll");

            // BepInEx / Reactor
            // TODO Install Reactor/BepInEx on the fly
            if (!Directory.Exists(installDir))
            {
                Error("BepInEx Not Installed", "BepInEx and Reactor are required for LevelImposter. \nBepInEx: https://docs.reactor.gg/docs/basic/install_bepinex \nReactor: https://docs.reactor.gg/docs/basic/install_reactor");
                return;
            }

            // Map File
            if (!File.Exists(MapHandler.mapDir))
            {
                Error("Invalid Map", "Thats weird...the map file you have selected is gone!");
                return;
            }

            // Write Files
            try
            {
                if (File.Exists(mapDir))
                    File.Delete(mapDir);
                File.Copy(MapHandler.mapDir, mapDir);
                
                if (!File.Exists(liDir))
                    File.WriteAllBytes(liDir, Resources.LevelImposter);
                if (!File.Exists(jsonDir))
                    File.WriteAllBytes(jsonDir, Resources.Newtonsoft);

                MessageBox.Show("Map has successfully been installed", "Success!", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch (Exception e)
            {
                Error("Error", "There was an error writing to Among Us: \n" + e.Message);
            }
        }

        private void Error(string title, string text)
        {
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
