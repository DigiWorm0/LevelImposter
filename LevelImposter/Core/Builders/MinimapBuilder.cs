using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LevelImposter.Core
{
    public class MinimapBuilder : Builder
    {
        public void Build(LIElement elem, GameObject obj)
        {
            if (elem.type != "util-minimap")
                return;

            ShipStatus shipStatus = LIShipStatus.Instance.shipStatus;
            MapBehaviour mapBehaviour = MapBehaviour.Instance;

            if (mapBehaviour == null)
            {
                mapBehaviour = UnityEngine.Object.Instantiate(shipStatus.MapPrefab, HudManager.Instance.transform);
                mapBehaviour.gameObject.SetActive(false);
            }

            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                GameObject background = mapBehaviour.ColorControl.gameObject;
                background.GetComponent<SpriteRenderer>().sprite = sprite;
                background.transform.localScale = obj.transform.localScale;
                background.transform.localRotation = obj.transform.localRotation;
            }

            Transform hereIndicatorParent = mapBehaviour.transform.FindChild("HereIndicatorParent");
            hereIndicatorParent.localPosition = new Vector3(0, 5.0f, -0.1f);

            Transform roomNames = mapBehaviour.transform.FindChild("RoomNames");
            roomNames.gameObject.SetActive(false);

            obj.SetActive(false);
        }

        public void PostBuild()
        {

        }
    }
}
