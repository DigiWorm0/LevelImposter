using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;

namespace LevelImposter.Core
{
    public class CamBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-cam")
                return;

            UtilData utilData = AssetDB.utils[elem.type];

            // Default Sprite
            obj.layer = (int)Layer.ShortObjects;
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;
            }
            spriteRenderer.material = utilData.SpriteRenderer.material;

            // Camera
            SurvCamera survCam = obj.AddComponent<SurvCamera>();
            survCam.CamName = elem.name;
            survCam.Offset = new Vector3(
                elem.properties.camXOffset == null ? 0 : (float)elem.properties.camXOffset,
                elem.properties.camYOffset == null ? 0 : (float)elem.properties.camYOffset
            );
            survCam.CamSize = elem.properties.camZoom == null ? 0 : (float)elem.properties.camZoom;
        }

        public void PostBuild() { }
    }
}
