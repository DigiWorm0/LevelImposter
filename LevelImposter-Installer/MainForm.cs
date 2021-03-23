using LevelImposter.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelImposter
{
    public partial class LevelImposter : Form
    {
        ApplyHandler  apply;
        BrowseHandler browse;
        RevertHandler revert;

        public LevelImposter()
        {
            InitializeComponent();

            this.apply  = new ApplyHandler(applyButton);
            this.browse = new BrowseHandler(browseButton, mapLabel);
            this.revert = new RevertHandler(revertButton);
        }
    }
}
