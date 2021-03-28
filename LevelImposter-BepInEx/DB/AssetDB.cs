using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LevelImposter.DB
{
    static class AssetDB
    {
        
        private static Dictionary<string, AssetData> db;

        public static void Init()
        {
            db = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, AssetData>>(
                Encoding.UTF8.GetString(Properties.Resources.AssetDB, 0, Properties.Resources.AssetDB.Length)
            );
        }

        public static bool Contains(string id)
        {
            return db.ContainsKey(id);
        }

        public static AssetData Get(string id)
        {
            return db.GetValueOrDefault(id);
        }

        public static void ImportMap(GameObject map)
        {
            ShipStatus shipStatus = map.GetComponent<ShipStatus>();
            foreach (var data in db)
            {
                AssetData assetData = data.Value;

                if (assetData.map != shipStatus.Type)
                    continue;
                if (assetData.spriteRenderer == null)
                {
                    GameObject obj = SearchChildren(map.transform, assetData.spriteRendererName);
                    if (obj != null)
                        assetData.spriteRenderer = obj.GetComponent<SpriteRenderer>();
                }
                if (assetData.mapObj == null)
                    assetData.mapObj = SearchChildren(map.transform, assetData.mapObjName);
                if (assetData.shipBehavior == null && !string.IsNullOrWhiteSpace(assetData.shipBehaviorName))
                {
                    CheckList(assetData, shipStatus.LongTasks);
                    CheckList(assetData, shipStatus.CommonTasks);
                    CheckList(assetData, shipStatus.NormalTasks);
                    //CheckList(assetData, shipStatus.AllVents);
                }

            }
        }

        // Search Children
        private static GameObject SearchChildren (Transform parent, string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            List<Transform> output = new List<Transform>();
            SearchChildren(parent, name, output);

            if (output.Count() <= 0)
                return null;
            return output[0].gameObject;
        }
        private static void SearchChildren(Transform parent, string name, List<Transform> output)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == name)
                {
                    output.Add(child);
                }
                SearchChildren(child, name, output);
            }
        }
        private static void CheckList<T> (AssetData data, UnhollowerBaseLib.Il2CppReferenceArray<T> list) where T : MonoBehaviour
        {
            if (data.shipBehavior != null)
                return;
            IEnumerable<T> elem = list.Where(t => t.name == data.shipBehaviorName);
            if (elem.Count() > 0)
                data.shipBehavior = elem.First();
        }
    }
}
