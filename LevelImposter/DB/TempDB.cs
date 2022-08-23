using System;
using System.Collections.Generic;

namespace LevelImposter.DB
{
    [Serializable]
    class TempDB
    {
        public Dictionary<string, TaskData> tasks { get; set; }
        public Dictionary<string, UtilData> utils { get; set; }
        public Dictionary<string, SabData> sabs { get; set; }
        public Dictionary<string, DecData> dec { get; set; }
        public Dictionary<string, RoomData> room { get; set; }
        public Dictionary<string, SSData> ss { get; set; }
        public Dictionary<string, SoundData> sounds { get; set; }
    }
}