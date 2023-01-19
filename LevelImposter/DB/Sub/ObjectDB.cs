using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LevelImposter.Core;
using System.Text.Json.Serialization;

namespace LevelImposter.DB
{
    /// <summary>
    /// Database of Among Us GameObjects
    /// </summary>
    public class ObjectDB : SubDB<GameObject>
    {
        public ObjectDB(SerializedAssetDB serializedDB) : base(serializedDB) { }

        public override void LoadShip(ShipStatus shipStatus, MapNames mapType)
        {
            DB.ObjectDB.ForEach((elem) =>
            {
                if (mapType != elem.MapType)
                    return;

                // Transform
                var transform = FollowPath(elem.Path, shipStatus.transform);
                if (transform == null)
                {
                    LILogger.Warn($"ObjectDB could not find {elem.ID} in {shipStatus.name}");
                    return;
                }

                Add(elem.ID, transform.gameObject);
            });
        }

        [Serializable]
        public class DBElement
        { 
            public string ID { get; set; }
            public string Path { get; set; }
            public int Map { get; set; }

            [JsonIgnore]
            public MapNames MapType => (MapNames)Map;
        }
    }
}
