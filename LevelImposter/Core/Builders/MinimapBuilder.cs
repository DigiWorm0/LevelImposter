using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

namespace LevelImposter.Core
{
    public class MinimapBuilder : Builder
    {
        public const float DEFAULT_SCALE = 4.975f;

        public static float mapScale = DEFAULT_SCALE;

        private bool isBuilt = false;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-minimap")
                return;
            if (isBuilt)
            {
                LILogger.Warn("Only 1 minimap object should be used per map");
                return;
            }

            float scale = elem.properties.minimapScale == null ? 1 : (float)elem.properties.minimapScale;
            mapScale = 1 / (scale * DEFAULT_SCALE);
            LIShipStatus.Instance.shipStatus.MapScale = scale * DEFAULT_SCALE;

            MapBehaviour mapBehaviour = GetMinimap();

            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                GameObject background = mapBehaviour.ColorControl.gameObject;
                background.GetComponent<SpriteRenderer>().sprite = sprite;
                background.transform.localPosition = background.transform.localPosition;
                background.transform.localScale = obj.transform.localScale * mapScale;
                background.transform.localRotation = obj.transform.localRotation;
            }

            Vector3 mapOffset = -(obj.transform.localPosition * mapScale);

            // Offsets
            Transform roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
            roomNames.localPosition = mapOffset;
            Transform hereIndicatorParent = mapBehaviour.transform.FindChild("HereIndicatorParent");
            hereIndicatorParent.localPosition = mapOffset + new Vector3(0, LIShipStatus.Y_OFFSET * mapScale, -0.1f);
            mapBehaviour.countOverlay.transform.localPosition = mapOffset;
            mapBehaviour.infectedOverlay.transform.localPosition = mapOffset;

            obj.SetActive(false);
            isBuilt = true;
        }

        public void PostBuild()
        {
            if (!isBuilt)
            {
                MapBehaviour mapBehaviour = GetMinimap();
                mapBehaviour.ColorControl.gameObject.SetActive(false);
                mapBehaviour.transform.FindChild("HereIndicatorParent").gameObject.SetActive(false);
                mapBehaviour.transform.FindChild("RoomNames").gameObject.SetActive(false);
            }
            isBuilt = false;
        }

        public static MapBehaviour GetMinimap()
        {
            MapBehaviour mapBehaviour = MapBehaviour.Instance;
            if (mapBehaviour == null)
            {
                mapBehaviour = UnityEngine.Object.Instantiate(LIShipStatus.Instance.shipStatus.MapPrefab, HudManager.Instance.transform);
                mapBehaviour.gameObject.SetActive(false);
            }
            return mapBehaviour;
        }
    }
}
