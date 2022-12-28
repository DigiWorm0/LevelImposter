using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace LevelImposter.Core
{
    public class MinimapBuilder : IElemBuilder
    {
        public const float DEFAULT_SCALE = 4.975f;

        private bool _isBuilt = false;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-minimap")
                return;
            if (_isBuilt)
            {
                LILogger.Warn("Only 1 minimap object should be used per map");
                return;
            }

            // Minimap
            MapBehaviour mapBehaviour = GetMinimap();

            // Map Scale
            float mapScaleVal = elem.properties.minimapScale == null ? 1 : (float)elem.properties.minimapScale;
            float mapScale = mapScaleVal * DEFAULT_SCALE;
            LIShipStatus.Instance.ShipStatus.MapScale = mapScale;
            Vector3 mapOffset = -(obj.transform.localPosition / mapScale);

            // Sprite Renderer
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                GameObject background = mapBehaviour.ColorControl.gameObject;
                SpriteRenderer bgRenderer = background.GetComponent<SpriteRenderer>();
                bgRenderer.sprite = sprite;
                bgRenderer.color = spriteRenderer.color;
                background.transform.localPosition = background.transform.localPosition;
                background.transform.localScale = obj.transform.localScale / mapScale;
                background.transform.localRotation = obj.transform.localRotation;
            }

            // Offsets
            Transform roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
            roomNames.localPosition = mapOffset;
            Transform hereIndicatorParent = mapBehaviour.transform.FindChild("HereIndicatorParent");
            hereIndicatorParent.localPosition = mapOffset + new Vector3(0, 0, -0.1f);
            mapBehaviour.countOverlay.transform.localPosition = mapOffset;
            mapBehaviour.infectedOverlay.transform.localPosition = mapOffset;

            obj.SetActive(false);
            _isBuilt = true;
        }

        public void PostBuild()
        {
            if (!_isBuilt)
            {
                MapBehaviour mapBehaviour = GetMinimap();
                mapBehaviour.ColorControl.gameObject.SetActive(false);
                mapBehaviour.transform.FindChild("HereIndicatorParent").gameObject.SetActive(false);
                mapBehaviour.transform.FindChild("RoomNames").gameObject.SetActive(false);
            }
            _isBuilt = false;
        }

        /// <summary>
        /// Get the current Minimap Behaviour
        /// </summary>
        /// <returns>The current Minimap Behaviour</returns>
        public static MapBehaviour GetMinimap()
        {
            MapBehaviour mapBehaviour = MapBehaviour.Instance;
            if (mapBehaviour == null)
            {
                mapBehaviour = UnityEngine.Object.Instantiate(LIShipStatus.Instance.ShipStatus.MapPrefab, HudManager.Instance.transform);
                mapBehaviour.gameObject.SetActive(false);
            }
            return mapBehaviour;
        }
    }
}
