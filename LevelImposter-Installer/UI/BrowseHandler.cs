using LevelImposter.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelImposter.UI
{
    class BrowseHandler
    {
        private Button browseButton;
        private Label mapLabel;
        private OpenFileDialog browseDialog;

        public BrowseHandler(Button browseButton, Label mapLabel)
        {
            this.browseButton = browseButton;
            this.mapLabel = mapLabel;

            this.browseButton.Click += new System.EventHandler(this.onClick);
            this.browseDialog = new OpenFileDialog();
        }

        public void onClick(object sender, EventArgs e)
        {
            DialogResult result = browseDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string dir = browseDialog.FileName;
                MapHandler.Load(dir);
                //this.mapLabel.Text = MapHandler.map.name;
            }
        }
    }
}
