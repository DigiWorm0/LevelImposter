using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LevelImposter.DB;
using PowerTools;
using LevelImposter.Core;

namespace LevelImposter.Builders
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
            var prefabCam = prefab.GetComponent<SurvCamera>();

            // Sprite
            MapUtils.CloneSprite(obj, prefab, true);

            // Camera
            SurvCamera survCam = obj.AddComponent<SurvCamera>();
            survCam.CamName = elem.name;
            survCam.Offset = new Vector3(
                elem.properties.camXOffset == null ? 0 : (float)elem.properties.camXOffset,
                elem.properties.camYOffset == null ? 0 : (float)elem.properties.camYOffset
            );
            survCam.CamSize = elem.properties.camZoom == null ? 3 : (float)elem.properties.camZoom;
            survCam.OnAnim = prefabCam.OnAnim;
            survCam.OffAnim = prefabCam.OffAnim;
        }

        public void PostBuild() { }
    }
}
