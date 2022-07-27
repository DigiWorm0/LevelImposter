using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class MinimapBuilder : Builder
    {
        private bool isBuilt = false;

        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-minimap")
                return;
            if (isBuilt)
            {
                LILogger.Warn("Only 1 util-minimap should be used per map");
                return;
            }

            MapBehaviour mapBehaviour = GetMinimap();

            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                GameObject background = mapBehaviour.ColorControl.gameObject;
                background.GetComponent<SpriteRenderer>().sprite = sprite;
                background.transform.localScale = obj.transform.localScale * 0.2f;
                background.transform.localRotation = obj.transform.localRotation;
            }

            Vector3 mapOffset = -(obj.transform.localPosition * 0.2f);

            Transform hereIndicatorParent = mapBehaviour.transform.FindChild("HereIndicatorParent");
            hereIndicatorParent.localPosition = mapOffset + new Vector3(0, 5.0f, -0.1f);
            mapBehaviour.countOverlay.transform.localPosition = mapOffset;
            mapBehaviour.infectedOverlay.transform.localPosition = mapOffset;

            Transform roomNames = mapBehaviour.transform.GetChild(mapBehaviour.transform.childCount - 1);
            roomNames.gameObject.SetActive(false);

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
