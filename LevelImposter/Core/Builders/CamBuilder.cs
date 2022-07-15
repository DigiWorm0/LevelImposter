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
            if (elem.properties.camXOffset != null && elem.properties.camYOffset != null)
            {
                survCam.Offset = new Vector3(
                    (float)elem.properties.camXOffset,
                    (float)elem.properties.camYOffset
                );
            }
            if (elem.properties.camZoom != null)
            {
                survCam.CamSize = (float)elem.properties.camZoom;
            }
        }

        public void PostBuild() { }
    }
}
