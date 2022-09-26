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

            UtilData utilData = AssetDB.utils[elem.type];

            // Default Sprite
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            GIFAnimator gifAnimator = obj.GetComponent<GIFAnimator>();
            obj.layer = (int)Layer.ShortObjects;
            if (!spriteRenderer)
            {
                spriteRenderer = obj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = utilData.SpriteRenderer.sprite;

                SpriteAnim spriteAnimClone = utilData.GameObj.GetComponent<SpriteAnim>();
                SpriteAnim spriteAnim = obj.AddComponent<SpriteAnim>();
                spriteAnim.Play(spriteAnimClone.m_defaultAnim, spriteAnimClone.Speed);

                if (elem.properties.color != null)
                    spriteRenderer.color = MapUtils.LIColorToColor(elem.properties.color);
            }
            else if (gifAnimator != null)
            {
                gifAnimator.Stop();
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
