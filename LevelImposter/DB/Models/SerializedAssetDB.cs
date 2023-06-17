using System;
using System.Collections.Generic;

namespace LevelImposter.DB
{
    [Serializable]
    public class SerializedAssetDB
    {
        public List<ObjectDB.DBElement> ObjectDB { get; set; }
        public List<TaskDB.DBElement> TaskDB { get; set; }
        public List<SoundDB.DBElement> SoundDB { get; set; }
        public List<PathDB.DBElement> PathDB { get; set; }
    }
}
