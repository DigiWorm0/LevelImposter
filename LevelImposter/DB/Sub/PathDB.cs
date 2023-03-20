using LevelImposter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace LevelImposter.DB
{
    /// <summary>
    /// Database of string paths for Transforms
    /// </summary>
    public class PathDB : SubDB<string[]>
    {
        public PathDB(SerializedAssetDB serializedDB) : base(serializedDB) { }

        public override void Load()
        {
            DB.PathDB.ForEach((elem) =>
            {
                Add(elem.ID, elem.Paths);
            });
        }

        [Serializable]
        public class DBElement
        {
            public string ID { get; set; }
            public string[] Paths { get; set; }
        }
    }
}
