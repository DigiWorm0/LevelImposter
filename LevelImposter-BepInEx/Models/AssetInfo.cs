using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Models
{
    class AssetInfo
    {
        public AssetInfo(string name, string sprite, int taskID, string type)
        {
            this.name = name;
            this.sprite = sprite;
            this.taskID = taskID;
            this.type = type;
        }

        public string name;
        public string sprite;
        public int taskID;
        public string type;
    }
}
