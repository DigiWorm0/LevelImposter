using System;
using System.Collections.Generic;
using LevelImposter.DB.Sub;

namespace LevelImposter.DB.Models;

[Serializable]
public class SerializedAssetDB
{
    public List<ObjectDB.DBElement>? ObjectDB { get; set; }
    public List<TaskDB.DBElement>? TaskDB { get; set; }
    public List<SoundDB.DBElement>? SoundDB { get; set; }
    public List<PathDB.DBElement>? PathDB { get; set; }
}