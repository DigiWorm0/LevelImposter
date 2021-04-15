using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    class CustomBuilder : Builder
    {
        private PolusHandler polus;

        public CustomBuilder(PolusHandler polus)
        {
            this.polus = polus;
        }

        public bool Build(MapAsset asset)
        {
            GameObject obj = new GameObject("Custom Asset");

            // Base64
            string base64 = asset.type.Substring(asset.type.IndexOf(",") + 1);
            byte[] data;
            try
            {
                data = System.Convert.FromBase64String(base64);
            }
            catch
            {
                LILogger.LogError("Could not parse custom asset texture");
                return false;
            }

            // Texture
            Texture2D tex = new Texture2D(1,1);
            ImageConversion.LoadImage(tex, data);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            obj.layer = (int)Layer.Ship;

            // Colliders
            AssetBuilder.BuildColliders(asset, obj);

            // Polus
            polus.Add(obj, asset, MapType.Skeld);
            return true;
        }
    }
}
