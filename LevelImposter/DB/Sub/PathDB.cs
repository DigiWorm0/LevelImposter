using System;

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
