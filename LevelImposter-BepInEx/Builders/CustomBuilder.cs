using LevelImposter.Map;
using LevelImposter.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Builders
{
    static class CustomBuilder
    {
        public static GameObject Build(string base64)
        {
            GameObject obj = new GameObject("Custom Asset");

            // Base64 Decode
            string newBase64 = base64.Substring(base64.IndexOf(",") + 1);
            byte[] data;
            try
            {
                data = System.Convert.FromBase64String(newBase64);
            }
            catch
            {
                LILogger.LogError("Could not parse custom asset texture");
                return obj;
            }

            // Texture
            Texture2D tex = new Texture2D(1,1);
            ImageConversion.LoadImage(tex, data);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            return obj;
        }
    }
}
