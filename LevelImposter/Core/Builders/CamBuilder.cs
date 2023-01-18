using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;
using PowerTools;

namespace LevelImposter.Core
{
    public class CamBuilder : IElemBuilder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-cam")
                return;

            // Prefab
            var prefab = AssetDB.GetObject(elem.type);
            if (prefab == null)
                return;
            var prefabRenderer = prefab.GetComponent<SpriteRenderer>();
            var prefabAnim = prefab.GetComponent<SpriteAnim>();

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = prefabRenderer.sprite;

                SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();
                spriteAnim.Play(prefabAnim.m_defaultAnim, prefabAnim.Speed);

                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            spriteRenderer.material = prefabRenderer.material;

            // Camera
            SurvCamera survCam = obj.AddComponent<SurvCamera>();
            survCam.CamName = elem.name;
            survCam.Offset = new Vector3(
                elem.properties.camXOffset == null ? 0 : (float)elem.properties.camXOffset,
                elem.properties.camYOffset == null ? 0 : (float)elem.properties.camYOffset
            );
            survCam.CamSize = elem.properties.camZoom == null ? 3 : (float)elem.properties.camZoom;
        }

        public void PostBuild() { }
    }
}
