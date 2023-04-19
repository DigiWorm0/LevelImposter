using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using LevelImposter.Core;

namespace LevelImposter.Builders
{
    public class MinimapSpriteBuilder : IElemBuilder
    {
        private static int _sabCount = 0;

        public static int SabCount => _sabCount;

        public MinimapSpriteBuilder()
        {
            _sabCount = 0;
        }

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-minimapsprite")
                return;

            // ShipStatus
            var shipStatus = LIShipStatus.Instance?.ShipStatus;
            if (shipStatus == null)
                throw new MissingShipException();

            // Minimap
            MapBehaviour mapBehaviour = MinimapBuilder.GetMinimap();
            InfectedOverlay infectedOverlay = mapBehaviour.infectedOverlay;
            MapTaskOverlay taskOverlay = mapBehaviour.taskOverlay;
            bool imposterOnly = elem.properties.imposterOnly == true;
            Transform parentTransform = imposterOnly ? infectedOverlay.transform : taskOverlay.transform;
            if (imposterOnly)
                _sabCount++;

            // GameObject
            float mapScale = shipStatus.MapScale;
            GameObject spriteObj = new(elem.name);
            spriteObj.layer = (int)Layer.UI;
            spriteObj.transform.SetParent(parentTransform);
            spriteObj.transform.localPosition = new Vector3(
                elem.x / mapScale,
                elem.y / mapScale,
                elem.z
            );
            spriteObj.transform.localScale = new Vector3(elem.xScale, elem.yScale, 1);
            spriteObj.transform.localRotation = Quaternion.Euler(0, 0, elem.rotation);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                LILogger.Warn("util-minimapsprite does not have a sprite attatched");
                return;
            }

            // Background
            if (SpriteLoader.Instance == null)
            {
                LILogger.Warn("Spite Loader is not instantiated");
                return;
            }
            SpriteRenderer bgRenderer = spriteObj.AddComponent<SpriteRenderer>();
            SpriteLoader.Instance.OnLoad += (LIElement loadedElem) =>
            {
                if (loadedElem.id != elem.id || bgRenderer == null)
                    return;
                bgRenderer.sprite = spriteRenderer.sprite;
                bgRenderer.color = spriteRenderer.color;
                UnityEngine.Object.Destroy(obj);
            };
        }

        public void PostBuild() { }
    }
}
