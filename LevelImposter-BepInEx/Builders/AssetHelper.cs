using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    static class AssetHelper
    {
        public static UnhollowerBaseLib.Il2CppReferenceArray<T> AddToArr<T>(UnhollowerBaseLib.Il2CppReferenceArray<T> arr, T value) where T : UnhollowerBaseLib.Il2CppObjectBase
        {
            List<T> list = new List<T>(arr);
            list.Add(value);
            return list.ToArray();
        }

        public static void BuildColliders(MapAsset asset, GameObject obj, float scale = 1.0f)
        {
            // Colliders
            GameObject shadowObj = new GameObject("Shadows");
            shadowObj.layer = (int)Layer.Shadow;
            shadowObj.transform.SetParent(obj.transform);
            foreach (MapCollider collider in asset.colliders)
            {
                EdgeCollider2D edgeCollider = obj.AddComponent<EdgeCollider2D>();
                edgeCollider.SetPoints(collider.GetPoints(scale, scale));

                if (collider.blocksLight)
                {
                    EdgeCollider2D lightCollider = shadowObj.AddComponent<EdgeCollider2D>();
                    lightCollider.SetPoints(collider.GetPoints(scale, scale));
                }
            }
        }

        public static Sprite SpriteFromBase64(byte[] data)
        {
            Texture2D tex = new Texture2D(1, 1);
            ImageConversion.LoadImage(tex, data);
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        public static Sprite SpriteFromBase64(string b64)
        {
            if (string.IsNullOrEmpty(b64))
                return null;

            // Base64
            string base64 = b64.Substring(b64.IndexOf(",") + 1);
            byte[] data;
            try
            {
                data = System.Convert.FromBase64String(base64);
                return SpriteFromBase64(data);
            }
            catch
            {
                LILogger.LogError("Could not parse custom asset texture");
                return null;
            }

         }

        public static void ClearChildren(Transform obj)
        {
            for (int i = 0; i < obj.childCount; i++)
                GameObject.Destroy(obj.GetChild(i).gameObject);
        }
    }
}
