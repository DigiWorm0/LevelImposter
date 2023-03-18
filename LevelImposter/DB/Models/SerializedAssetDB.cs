using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
