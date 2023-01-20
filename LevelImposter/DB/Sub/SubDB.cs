using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LevelImposter.Core;

namespace LevelImposter.DB
{
    public abstract class SubDB<T>
    {
        private Dictionary<string, T> _data = new();
        private SerializedAssetDB _serializedDB;

        public SerializedAssetDB DB => _serializedDB;

        public SubDB(SerializedAssetDB serializedDB)
        {
            _serializedDB = serializedDB;
        }

        /// <summary>
        /// Loads any extra assets into the DB
        /// </summary>
        public virtual void Load() { }

        /// <summary>
        /// Loads each ship into the DB
        /// </summary>
        /// <param name="shipStatus">ShipStatus to load</param>
        /// <param name="mapType">MapType of ShipStatus</param>
        public virtual void LoadShip(ShipStatus shipStatus, MapType mapType) { }

        /// <summary>
        /// Gets an object from the DB
        /// </summary>
        /// <param name="id">Type ID of the object</param>
        /// <returns>Object or null if not found</returns>
        public T? Get(string id)
        {
            _data.TryGetValue(id, out T? result);
            return result;
        }

        /// <summary>
        /// Adds an object to the DB
        /// </summary>
        /// <param name="id">ID of the object</param>
        /// <param name="obj">Object to add</param>
        public void Add(string id, T obj)
        {
            _data.Add(id, obj);
        }

        public Transform FollowPath(string path, Transform parent)
        {
            List<string> parentName = new(path.Split("/"));
            Transform? transform = parent;
            parentName.ForEach((name) =>
            {
                if (transform != null)
                    transform = transform.Find(name);
            });
            return transform;
        }
    }
}
