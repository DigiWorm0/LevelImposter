using LevelImposter.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelImposter.UI
{
    class RevertHandler
    {
        private Button revertButton;

        public RevertHandler(Button revertButton)
        {
            this.revertButton = revertButton;
            this.revertButton.Click += new System.EventHandler(this.onClick);
        }

        public void onClick(object sender, EventArgs e)
        {
            this.revertButton.Enabled = false;
            AssetFileHandler.Revert();
            this.revertButton.Enabled = true;
        }
    }
}
