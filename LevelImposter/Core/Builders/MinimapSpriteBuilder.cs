using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace LevelImposter.Core
{
    public class MinimapSpriteBuilder : IElemBuilder
    {

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-minimapsprite")
                return;

            // Minimap
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;
            MapTaskOverlay taskOverlay = mapBehaviour.taskOverlay;
            bool imposterOnly = elem.properties.imposterOnly == true;
            Transform parentTransform = imposterOnly ? infectedOverlay.transform : taskOverlay.transform;

            // GameObject
            float mapScale = LIShipStatus.Instance.ShipStatus.MapScale;
            GameObject spriteObj = new(elem.name);
            spriteObj.layer = (int)Layer.UI;
            spriteObj.transform.SetParent(parentTransform);
            spriteObj.transform.localPosition = new Vector3(
                elem.x / mapScale,
                elem.y / mapScale,
                -25.0f
            );
            spriteObj.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1);
            spriteObj.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);

            // Sprite Renderer
            LISpriteLoader spriteLoader = obj.GetComponent<LISpriteLoader>();
            if (spriteLoader == null)
            {
                LILogger.Warn("util-minimapsprite does not have a sprite attatched");
                return;
            }
            SpriteRenderer spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
            spriteLoader.OnLoad.AddListener((Action<Sprite>)((Sprite sprite) =>
            {
                spriteRenderer.sprite = sprite;
                UnityEngine.Object.Destroy(obj);
            }));
        }

        public void PostBuild() { }
    }
}
